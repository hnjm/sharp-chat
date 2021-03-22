using SharpChat.Sessions;

namespace SharpChat.Events {
    public class SessionDestroyEvent : Event {
        public const string TYPE = @"session:destroy";

        public override string Type => TYPE;
        public string ServerId { get; }
        public string SessionId { get; }

        public SessionDestroyEvent(ISession session) : base(null, session.User) { // user isn't really needed, should prolly be NULL
            ServerId = session.ServerId;
            SessionId = session.SessionId;
        }
    }
}
