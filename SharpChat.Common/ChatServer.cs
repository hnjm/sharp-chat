using Hamakaze;
using SharpChat.Commands;
using SharpChat.Configuration;
using SharpChat.Database;
using SharpChat.DataProvider;
using SharpChat.PacketHandlers;
using SharpChat.Packets;
using SharpChat.RateLimiting;
using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat {
    public class ChatServer : IDisposable {
        public const int EXT_VERSION =
#if DEBUG
            2;
#else
            1;
#endif

        public const int DEFAULT_MAX_CONNECTIONS = 5;

        private IConfig Config { get; }
        private IWebSocketServer Server { get; }
        private ChatContext Context { get; }
        private DatabaseWrapper Database { get; }
        private RateLimiter RateLimiter { get; }

        public HttpClient HttpClient { get; }

        private IReadOnlyCollection<IPacketHandler> PacketHandlers { get; }
        private CachedValue<int> FloodBanDuration { get; }
        private CachedValue<int> FloodRankException { get; }

        public bool AcceptingConnections { get; private set; }

        public ChatServer(IConfig config, IWebSocketServer server, HttpClient httpClient, IDataProvider dataProvider, IDatabaseBackend databaseBackend) {
            Logger.Write("Starting Sock Chat server...");

            Config = config ?? throw new ArgumentNullException(nameof(config));
            Database = new DatabaseWrapper(databaseBackend ?? throw new ArgumentNullException(nameof(databaseBackend)));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Context = new ChatContext(Config.ScopeTo(@"chat"), Database, dataProvider);
            RateLimiter = new RateLimiter(Config.ScopeTo(@"chat:flood"));

            FloodBanDuration = Config.ReadCached(@"chat:flood:banDuration", RateLimiter.DEFAULT_BAN_DURATION);
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

            Context.Update();
        }

        private void OnClose(IWebSocketConnection conn) {
            Context.Update();
        }

        private void OnError(IWebSocketConnection conn, Exception ex) {
            Session sess = Context.Sessions.ByConnection(conn);
            string sessId = sess?.Id ?? new string('0', Session.ID_LENGTH);
            Logger.Write($@"[{sessId} {conn.RemoteAddress}] {ex}");
            Context.Update();
        }

        private void OnMessage(IWebSocketConnection conn, string msg) {
            Context.Update();

            RateLimitState rateLimit = RateLimiter.Bump(conn);
            Logger.Debug($@"[{conn.RemoteAddress}] {rateLimit}");
            if(rateLimit == RateLimitState.Disconnect) {
                conn.Dispose();
                return;
            }

            IEnumerable<string> args = msg.Split('\t');
            if(!Enum.TryParse(args.ElementAtOrDefault(0), out SockChatClientPacket opCode))
                return;

            Session sess = Context.Sessions.ByConnection(conn);

            if(opCode != SockChatClientPacket.Authenticate) {
                if(sess == null) {
                    conn.Dispose();
                    return;
                }

                if(rateLimit == RateLimitState.Kick) {
                    Context.BanUser(sess.User, DateTimeOffset.Now.AddSeconds(FloodBanDuration), false, UserDisconnectReason.Flood);
                    return;
                } else if(rateLimit == RateLimitState.Warning)
                    sess.User.Send(new FloodWarningPacket());
            }

            PacketHandlers.FirstOrDefault(x => x.PacketId == opCode)?.HandlePacket(
                new PacketHandlerContext(args, Context, sess, conn)
            );
        }

        private bool IsDisposed;
        ~ChatServer()
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
