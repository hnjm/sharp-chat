using System;

namespace SharpChat.Events {
    public class UserConnectEvent : IChatEvent {
        public DateTimeOffset DateTime { get; private set; }

        public ChatUser Sender { get; private set; } 

        public IPacketTarget Target { get; private set; }

        public ChatMessageFlags Flags { get; private set; } = ChatMessageFlags.Log;

        public long SequenceId { get; set; }

        public UserConnectEvent(DateTimeOffset joined, ChatUser user, IPacketTarget target) {
            DateTime = joined;
            Sender = user;
            Target = target;
        }
    }
}
