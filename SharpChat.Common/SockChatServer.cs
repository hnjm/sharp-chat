using Hamakaze;
using SharpChat.Channels;
using SharpChat.Commands;
using SharpChat.Configuration;
using SharpChat.Database;
using SharpChat.DataProvider;
using SharpChat.PacketHandlers;
using SharpChat.Packets;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat {
    public class SockChatServer : IDisposable {
        public const int EXT_VERSION = 2;

        public const int DEFAULT_MAX_CONNECTIONS = 5;
        public const int DEFAULT_FLOOD_BAN_DURATION = 30;

        public static ChatUser Bot { get; } = new ChatUser {
            UserId = -1,
            Username = @"ChatBot",
            Rank = 0,
            Colour = new ChatColour(),
        };

        private IConfig Config { get; }
        private IWebSocketServer Server { get; }
        private ChatContext Context { get; }
        private DatabaseWrapper Database { get; }

        public HttpClient HttpClient { get; }

        private IReadOnlyCollection<IPacketHandler> PacketHandlers { get; }
        private CachedValue<int> FloodBanDuration { get; }
        private CachedValue<int> FloodRankException { get; }
        private CachedValue<int> MaxConnectionsValue { get; }

        public int MaxConnections => MaxConnectionsValue;

        public List<ChatUserSession> Sessions { get; } = new List<ChatUserSession>();
        private object SessionsLock { get; } = new object();

        public ChatUserSession GetSession(IWebSocketConnection conn) {
            lock(SessionsLock)
                return Sessions.FirstOrDefault(x => x.Connection == conn);
        }

        public SockChatServer(IConfig config, IWebSocketServer server, HttpClient httpClient, IDataProvider dataProvider, IDatabaseBackend databaseBackend) {
            Logger.Write("Starting Sock Chat server...");

            Config = config ?? throw new ArgumentNullException(nameof(config));
            Database = new DatabaseWrapper(databaseBackend ?? throw new ArgumentNullException(nameof(databaseBackend)));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Context = new ChatContext(this, Config.ScopeTo(@"chat"), Database, dataProvider);

            FloodBanDuration = Config.ReadCached(@"chat:flood:banDuration", DEFAULT_FLOOD_BAN_DURATION);
            FloodRankException = Config.ReadCached(@"chat:flood:exceptRank", 0, TimeSpan.FromSeconds(10));
            MaxConnectionsValue = config.ReadCached(@"chat:users:maxConnections", DEFAULT_MAX_CONNECTIONS);

            Context.Channels.Add(new ChatChannel(@"Lounge"));
#if DEBUG
            Context.Channels.Add(new ChatChannel(@"Programming"));
            Context.Channels.Add(new ChatChannel(@"Games"));
            Context.Channels.Add(new ChatChannel(@"Splatoon"));
            Context.Channels.Add(new ChatChannel(@"Password") { Password = @"meow", });
#endif
            Context.Channels.Add(new ChatChannel(@"Staff") { Rank = 5 });

            PacketHandlers = new IPacketHandler[] {
                new PingPacketHandler(),
                new AuthPacketHandler(this),
                new MessageSendPacketHandler(Context, new IChatCommand[] {
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
#if DEBUG
                new TypingPacketHandler(),
#endif
            };

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

            int exceptRank = FloodRankException;
            if(sess.User is ChatUser && (exceptRank <= 0 || sess.User.Rank < exceptRank)) {
                sess.User.RateLimiter.AddTimePoint();

                if(sess.User.RateLimiter.State == ChatRateLimitState.Kick) {
                    Context.BanUser(sess.User, DateTimeOffset.UtcNow.AddSeconds(FloodBanDuration), false, UserDisconnectReason.Flood);
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
            FloodBanDuration.Dispose();
            FloodRankException.Dispose();
            MaxConnectionsValue.Dispose();
        }
    }
}
