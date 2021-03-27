using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;

namespace SharpChat.PacketHandlers {
    public interface IPacketHandlerContext {
        IEnumerable<string> Args { get; }
        IConnection Connection { get; }
        ISession Session { get; }
        IUser User { get; }

        bool HasSession { get; }
        bool HasUser { get; }
    }

    public class PacketHandlerContext : IPacketHandlerContext {
        public IEnumerable<string> Args { get; }
        public ISession Session { get; }
        public IConnection Connection { get; }

        public IUser User => Session.User;

        public bool HasSession => Session != null;
        public bool HasUser => HasSession;

        public PacketHandlerContext(IEnumerable<string> args, ISession sess, IConnection conn) {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            Session = sess;
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
        }
    }
}
