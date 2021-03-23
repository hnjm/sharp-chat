using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionPingEvent : SessionEvent {
        public const string TYPE = PREFIX + @"ping";

        public SessionPingEvent(ISession session)
            : base(session, false) { }
    }
}
