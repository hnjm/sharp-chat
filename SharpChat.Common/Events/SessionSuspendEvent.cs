using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionSuspendEvent : SessionEvent {
        public const string TYPE = PREFIX + @"suspend";

        public SessionSuspendEvent(ISession session)
            : base(session, false, null) { }
    }
}
