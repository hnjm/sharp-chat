using SharpChat.Channels;
using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionChannelSwitchEvent : SessionEvent {
        public const string TYPE = PREFIX + @":channel:switch";

        public SessionChannelSwitchEvent(ISession session, IChannel channel)
            : base(session, false, channel) { }
    }
}
