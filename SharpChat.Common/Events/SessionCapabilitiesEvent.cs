using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionCapabilitiesEvent : SessionEvent {
        public const string TYPE = PREFIX + @"setcaps";

        public ClientCapability Capabilities { get; }

        public SessionCapabilitiesEvent(ISession session, ClientCapability caps) : base(session, false) {
            Capabilities = caps;
        }
    }
}
