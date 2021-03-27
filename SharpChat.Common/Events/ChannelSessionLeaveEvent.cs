using SharpChat.Channels;
using SharpChat.Sessions;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelSessionLeaveEvent : Event {
        public const string TYPE = @"channel:session:leave";

        public string SessionId { get; }

        public ChannelSessionLeaveEvent(IChannel channel, ISession session)
            : base(
                  channel ?? throw new ArgumentNullException(nameof(channel)),
                  session?.User ?? throw new ArgumentNullException(nameof(session))
              ) {
            SessionId = session.SessionId;
        }
    }
}
