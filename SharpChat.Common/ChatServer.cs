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
                new AuthPacketHandler(Context.Sessions, VERSION),
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

            if(VERSION >= 2)
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

        private void OnOpen(IConnection conn) {
            Logger.Debug($@"[{conn}] Connection oepened");

            if(!AcceptingConnections) {
                conn.Dispose();
                return;
            }

            Context.Update();
        }

        private void OnClose(IConnection conn) {
            Logger.Debug($@"[{conn}] Connection closed");
            Context.Update();
        }

        private void OnError(IConnection conn, Exception ex) {
            Session sess = Context.Sessions.ByConnection(conn);
            Logger.Write($@"[{sess} {conn}] {ex}");
            Context.Update();
        }

        private void OnMessage(IConnection conn, string msg) {
            Context.Update();

            RateLimitState rateLimit = Context.RateLimiter.Bump(conn);
            Logger.Debug($@"[{conn}] {rateLimit}");
            if(rateLimit == RateLimitState.Disconnect) {
                conn.Dispose();
                return;
            }

            IEnumerable<string> args = msg.Split(IServerPacket.SEPARATOR);
            if(!Enum.TryParse(args.ElementAtOrDefault(0), out ClientPacket opCode))
                return;

            Session sess = Context.Sessions.ByConnection(conn);

            if(opCode != ClientPacket.Authenticate) {
                if(sess == null) {
                    conn.Dispose();
                    return;
                }

                if(rateLimit == RateLimitState.Kick) {
                    Context.BanUser(sess.User, DateTimeOffset.Now + Context.RateLimiter.BanDuration, false, UserDisconnectReason.Flood);
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
        }
    }
}
