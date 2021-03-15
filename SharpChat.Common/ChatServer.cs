using SharpChat.Commands;
using SharpChat.Configuration;
using SharpChat.Database;
using SharpChat.DataProvider;
using SharpChat.PacketHandlers;
using SharpChat.Packets;
using SharpChat.RateLimiting;
using SharpChat.Sessions;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat {
    public class ChatServer : IDisposable {
        private const int VERSION =
#if DEBUG
            2;
#else
            1;
#endif

        public const int DEFAULT_MAX_CONNECTIONS = 5;

        private IConfig Config { get; }
        private IServer Server { get; }
        private ChatContext Context { get; }

        private IReadOnlyCollection<IPacketHandler> PacketHandlers { get; }

        public bool AcceptingConnections { get; private set; }

        public ChatServer(IConfig config, IServer server, IDataProvider dataProvider, IDatabaseBackend databaseBackend) {
            Logger.Write("Starting Sock Chat server...");

            Config = config ?? throw new ArgumentNullException(nameof(config));
            Context = new ChatContext(Config.ScopeTo(@"chat"), databaseBackend, dataProvider);

            List<IPacketHandler> handlers = new List<IPacketHandler> {
                new PingPacketHandler(),
                new AuthPacketHandler(Context.Sessions, Context.Bot, VERSION),
                new MessageSendPacketHandler(Context, new ICommand[] {
                    new JoinCommand(),
                    new AFKCommand(),
                    new WhisperCommand(),
                    new ActionCommand(),
                    new WhoCommand(Context.Bot),
                    new DeleteMessageCommand(Context.Events),

                    new NickCommand(Context.Bot),
                    new CreateChannelCommand(Context.Bot),
                    new DeleteChannelCommand(Context.Bot),
                    new ChannelPasswordCommand(Context.Bot),
                    new ChannelRankCommand(Context.Bot),

                    new BroadcastCommand(Context.Bot),
                    new KickBanUserCommand(),
                    new PardonUserCommand(Context.Bot),
                    new PardonIPCommand(Context.Bot),
                    new BanListCommand(Context.Bot),
                    new WhoIsUserCommand(Context.Bot),
                    new SilenceUserCommand(Context.Bot),
                    new UnsilenceUserCommand(Context.Bot),
                }),
            };

            if(VERSION >= 2)
                handlers.AddRange(new IPacketHandler[] {
                    new CapabilitiesPacketHandler(),
                    new TypingPacketHandler(),
                });

            PacketHandlers = handlers.ToArray();

            Server = server ?? throw new ArgumentNullException(nameof(server));
            Server.OnOpen += OnOpen;
            Server.OnClose += OnClose;
            Server.OnError += OnError;
            Server.OnMessage += OnMessage;
            Server.Start();

            AcceptingConnections = true;
        }

        private void OnOpen(IConnection conn) {
            Logger.Debug($@"[{conn}] Connection opened");

            if(!AcceptingConnections) {
                conn.Dispose();
                return;
            }

            Context.Update();
        }

        private void OnClose(IConnection conn) {
            Logger.Debug($@"[{conn}] Connection closed");
            Context.RateLimiter.ClearConnection(conn);
            Context.Update();
        }

        private void OnError(IConnection conn, Exception ex) {
            Session sess = Context.Sessions.ByConnection(conn);
            Logger.Write($@"[{sess} {conn}] {ex}");
            Context.Update();
        }

        private void OnMessage(IConnection conn, string msg) {
            Context.Update();

            Session sess = Context.Sessions.ByConnection(conn);
            bool hasUser = sess?.HasUser == true;

            RateLimitState rateLimit = RateLimitState.None;
            if(!hasUser || !Context.RateLimiter.HasRankException(sess.User))
                rateLimit = Context.RateLimiter.BumpConnection(conn);
            
            Logger.Debug($@"[{conn}] {rateLimit}");
            if(!hasUser && rateLimit == RateLimitState.Drop) {
                conn.Dispose();
                return;
            }

            IEnumerable<string> args = msg.Split(IServerPacket.SEPARATOR);
            if(!Enum.TryParse(args.ElementAtOrDefault(0), out ClientPacket opCode))
                return;

            if(opCode != ClientPacket.Authenticate) {
                if(!hasUser) 
                    return;

                if(rateLimit == RateLimitState.Drop) {
                    Context.BanUser(sess.User, Context.RateLimiter.BanDuration, UserDisconnectReason.Flood);
                    return;
                } else if(rateLimit == RateLimitState.Warn)
                    sess.User.SendPacket(new FloodWarningPacket(Context.Bot));
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
        }
    }
}
