using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;

namespace SharpChat.PacketHandlers {
    public interface IPacketHandlerContext {
        IEnumerable<string> Args { get; }
        ChatContext Chat { get; }
        IWebSocketConnection Connection { get; }
        Session Session { get; }
        ChatUser User { get; }

        bool HasUser { get; }
    }

    public class PacketHandlerContext : IPacketHandlerContext {
        public IEnumerable<string> Args { get; }
        public ChatContext Chat { get; }
        public Session Session { get; }

        public IWebSocketConnection Connection => Session.Connection;
        public ChatUser User => Session.User;

        public bool HasUser => Session.HasUser;

        public PacketHandlerContext(IEnumerable<string> args, ChatContext ctx, Session sess) {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            Chat = ctx ?? throw new ArgumentNullException(nameof(ctx));
            Session = sess ?? throw new ArgumentNullException(nameof(sess));
        }
    }
}
