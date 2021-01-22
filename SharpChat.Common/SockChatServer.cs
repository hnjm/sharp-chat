using SharpChat.Channels;
using SharpChat.Commands;
using SharpChat.Database;
using SharpChat.DataProvider;
using SharpChat.PacketHandlers;
using SharpChat.Packets;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SharpChat {
    public class SockChatServer : IDisposable {
        public const int EXT_VERSION = 2;
        public const int MSG_LENGTH_MAX = 2100;

#if DEBUG
        public const int MAX_CONNECTIONS = 9001;
        public const int FLOOD_KICK_LENGTH = 5;
        public const bool ENABLE_TYPING_EVENT = true;
#else
        public const int MAX_CONNECTIONS = 5;
        public const int FLOOD_KICK_LENGTH = 30;
        public const bool ENABLE_TYPING_EVENT = false;
#endif

        public static ChatUser Bot { get; } = new ChatUser {
            UserId = -1,
            Username = @"ChatBot",
            Rank = 0,
            Colour = new ChatColour(),
        };

        public IWebSocketServer Server { get; }
        public DatabaseWrapper Database { get; }
        public ChatContext Context { get; }

        public HttpClient HttpClient { get; }

        private IReadOnlyCollection<IPacketHandler> PacketHandlers { get; } = new IPacketHandler[] {
            new PingPacketHandler(),
            new AuthPacketHandler(),
            new MessageSendPacketHandler(new IChatCommand[] {
                new JoinCommand(),
                new AFKCommand(),
                new WhisperCommand(),
                new ActionCommand(),
                new WhoCommand(),
                new DeleteMessageCommand(),

                new NickCommand(),
                new CreateChannelCommand(),
                new DeleteChannelCommand(),
                new ChannelPasswordCommand(),
                new ChannelRankCommand(),

                new BroadcastCommand(),
                new KickBanUserCommand(),
                new PardonUserCommand(),
                new PardonIPCommand(),
                new BanListCommand(),
                new WhoIsUserCommand(),
                new SilenceUserCommand(),
                new UnsilenceUserCommand(),
            }),
            new TypingPacketHandler(),
        };

        public List<ChatUserSession> Sessions { get; } = new List<ChatUserSession>();
        private object SessionsLock { get; } = new object();

        public ChatUserSession GetSession(IWebSocketConnection conn) {
            lock(SessionsLock)
                return Sessions.FirstOrDefault(x => x.Connection == conn);
        }

        public SockChatServer(IWebSocketServer server, HttpClient httpClient, IDataProvider dataProvider, IDatabaseBackend databaseBackend) {
            Logger.Write("Starting Sock Chat server...");

            Database = new DatabaseWrapper(databaseBackend ?? throw new ArgumentNullException(nameof(databaseBackend)));

            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            Context = new ChatContext(this, dataProvider);

            Context.Channels.Add(new ChatChannel(@"Lounge"));
#if DEBUG
            Context.Channels.Add(new ChatChannel(@"Programming"));
            Context.Channels.Add(new ChatChannel(@"Games"));
            Context.Channels.Add(new ChatChannel(@"Splatoon"));
            Context.Channels.Add(new ChatChannel(@"Password") { Password = @"meow", });
#endif
            Context.Channels.Add(new ChatChannel(@"Staff") { Rank = 5 });

            Server = server ?? throw new ArgumentNullException(nameof(server));
            Server.OnOpen += OnOpen;
            Server.OnClose += OnClose;
            Server.OnError += OnError;
            Server.OnMessage += OnMessage;
            Server.Start();
        }

        private void OnOpen(IWebSocketConnection conn) {
            lock(SessionsLock) {
                if(!Sessions.Any(x => x.Connection == conn))
                    Sessions.Add(new ChatUserSession(conn));
            }

            Context.Update();
        }

        private void OnClose(IWebSocketConnection conn) {
            ChatUserSession sess = GetSession(conn);

            // Remove connection from user
            if(sess?.User != null) {
                // RemoveConnection sets conn.User to null so we must grab a local copy.
                ChatUser user = sess.User;

                user.RemoveSession(sess);

                if(!user.HasSessions)
                    Context.UserLeave(null, user);
            }

            // Update context
            Context.Update();

            // Remove connection from server
            lock(SessionsLock)
                Sessions.Remove(sess);

            sess?.Dispose();
        }

        private void OnError(IWebSocketConnection conn, Exception ex) {
            ChatUserSession sess = GetSession(conn);
            string sessId = sess?.Id ?? new string('0', ChatUserSession.ID_LENGTH);
            Logger.Write($@"[{sessId} {conn.RemoteAddress}] {ex}");
            Context.Update();
        }

        private void OnMessage(IWebSocketConnection conn, string msg) {
            Context.Update();

            ChatUserSession sess = GetSession(conn);

            if(sess == null) {
                conn.Dispose();
                return;
            }

            if(sess.User is ChatUser && sess.User.HasFloodProtection) {
                sess.User.RateLimiter.AddTimePoint();

                if(sess.User.RateLimiter.State == ChatRateLimitState.Kick) {
                    Context.BanUser(sess.User, DateTimeOffset.UtcNow.AddSeconds(FLOOD_KICK_LENGTH), false, UserDisconnectReason.Flood);
                    return;
                } else if(sess.User.RateLimiter.State == ChatRateLimitState.Warning)
                    sess.User.Send(new FloodWarningPacket()); // make it so this thing only sends once
            }

            string[] args = msg.Split('\t');

            if(args.Length < 1 || !Enum.TryParse(args[0], out SockChatClientPacket opCode))
                return;

            PacketHandlers.FirstOrDefault(x => x.PacketId == opCode)?.HandlePacket(
                new PacketHandlerContext(args, Context, sess)
            );
        }

        private bool IsDisposed;

        ~SockChatServer()
            => DoDispose();

        public void Dispose() { 
            DoDispose();
            GC.SuppressFinalize(this);
        }

        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;

            Sessions?.Clear();
            Server?.Dispose();
            Context?.Dispose();
            HttpClient?.Dispose();
            Database?.Dispose();
        }
    }
}
