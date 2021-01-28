using Hamakaze;
using SharpChat.Commands;
using SharpChat.Configuration;
using SharpChat.Database;
using SharpChat.DataProvider;
using SharpChat.PacketHandlers;
using SharpChat.Packets;
using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat {
    public class SockChatServer : IDisposable {
        public const int EXT_VERSION =
#if DEBUG
            2;
#else
            1;
#endif

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

        public bool AcceptingConnections { get; private set; }

        public SockChatServer(IConfig config, IWebSocketServer server, HttpClient httpClient, IDataProvider dataProvider, IDatabaseBackend databaseBackend) {
            Logger.Write("Starting Sock Chat server...");

            Config = config ?? throw new ArgumentNullException(nameof(config));
            Database = new DatabaseWrapper(databaseBackend ?? throw new ArgumentNullException(nameof(databaseBackend)));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Context = new ChatContext(Config.ScopeTo(@"chat"), Database, dataProvider);

            FloodBanDuration = Config.ReadCached(@"chat:flood:banDuration", DEFAULT_FLOOD_BAN_DURATION);
            FloodRankException = Config.ReadCached(@"chat:flood:exceptRank", 0, TimeSpan.FromSeconds(10));

            List<IPacketHandler> handlers = new List<IPacketHandler> {
                new PingPacketHandler(),
                new AuthPacketHandler(Context.Sessions),
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
            };

            if(EXT_VERSION >= 2)
                handlers.Add(new TypingPacketHandler());

            PacketHandlers = handlers.ToArray();

            Server = server ?? throw new ArgumentNullException(nameof(server));
            Server.OnOpen += OnOpen;
            Server.OnClose += OnClose;
            Server.OnError += OnError;
            Server.OnMessage += OnMessage;
            Server.Start();

            AcceptingConnections = true;
        }

        private void OnOpen(IWebSocketConnection conn) {
            if(!AcceptingConnections) {
                conn.Dispose();
                return;
            }

            // Recreation of old behaviour, should be altered for resumable sessions (as in not be here)
            Context.Sessions.FindMany(s => s.Connection == conn, sessions => {
                if(!sessions.Any())
                    Context.Sessions.Add(new Session(conn));
            });

            Context.Update();
        }

        private void OnClose(IWebSocketConnection conn) {
            Session sess = Context.Sessions.ByConnection(conn);

            if(sess != null) {
                // Remove connection from user
                if(sess.HasUser) {
                    // RemoveConnection sets conn.User to null so we must grab a local copy.
                    ChatUser user = sess.User;

                    user.RemoveSession(sess);

                    if(Context.Sessions.GetSessionCount(user) < 1)
                        Context.UserLeave(null, user);
                }

                Context.Update();
            }
        }

        private void OnError(IWebSocketConnection conn, Exception ex) {
            Session sess = Context.Sessions.ByConnection(conn);
            string sessId = sess?.Id ?? new string('0', Session.ID_LENGTH);
            Logger.Write($@"[{sessId} {conn.RemoteAddress}] {ex}");
            Context.Update();
        }

        private void OnMessage(IWebSocketConnection conn, string msg) {
            Context.Update();

            Session sess = Context.Sessions.ByConnection(conn);
            if(sess == null) {
                conn.Dispose();
                return;
            }

            int exceptRank = FloodRankException;
            if(sess.User is ChatUser && (exceptRank <= 0 || sess.User.Rank < exceptRank)) {
                sess.User.RateLimiter.AddTimePoint();

                if(sess.User.RateLimiter.State == ChatRateLimitState.Kick) {
                    Context.BanUser(sess.User, DateTimeOffset.Now.AddSeconds(FloodBanDuration), false, UserDisconnectReason.Flood);
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

            AcceptingConnections = false;

            Context.Dispose();
            Server.Dispose();

            FloodBanDuration.Dispose();
            FloodRankException.Dispose();
        }
    }
}
