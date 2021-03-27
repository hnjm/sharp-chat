using SharpChat.Channels;
using SharpChat.Sessions;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelSessionJoinEvent : Event {
        public const string TYPE = @"channel:session:join";

        public string SessionId { get; }

        public ChannelSessionJoinEvent(IChannel channel, ISession session)
            : base(
                  channel ?? throw new ArgumentNullException(nameof(channel)),
                  session?.User ?? throw new ArgumentNullException(nameof(session))
             ) {
            SessionId = session.SessionId;
        }
    }
}
