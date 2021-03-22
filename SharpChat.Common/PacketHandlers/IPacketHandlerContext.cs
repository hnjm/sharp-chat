using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;

namespace SharpChat.PacketHandlers {
    public interface IPacketHandlerContext {
        IEnumerable<string> Args { get; }
        ChatContext Chat { get; }
        IConnection Connection { get; }
        ILocalSession Session { get; }
        IUser User { get; }

        bool HasSession { get; }
        bool HasUser { get; }
    }

    public class PacketHandlerContext : IPacketHandlerContext {
        public IEnumerable<string> Args { get; }
        public ChatContext Chat { get; }
        public ILocalSession Session { get; }
        public IConnection Connection { get; }

        public IUser User => Session.User;

        public bool HasSession => Session != null;
        public bool HasUser => HasSession;

        public PacketHandlerContext(IEnumerable<string> args, ChatContext ctx, ILocalSession sess, IConnection conn) {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            Chat = ctx ?? throw new ArgumentNullException(nameof(ctx));
            Session = sess;
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
        }
    }
}
