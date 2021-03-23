using SharpChat.Channels;
using SharpChat.Sessions;

namespace SharpChat.Events {
    public abstract class SessionEvent : Event {
        public const string PREFIX = @"session:";

        public string ServerId { get; }
        public string SessionId { get; }

        public SessionEvent(ISession session, bool includeUser = false, IChannel channel = null)
            : base(channel, includeUser ? session.User : null) {
            ServerId = session.ServerId;
            SessionId = session.SessionId;
        }
    }
}
