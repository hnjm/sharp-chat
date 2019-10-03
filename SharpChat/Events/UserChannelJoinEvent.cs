using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Events {
    public class UserChannelJoinEvent : IChatEvent {
        public DateTimeOffset DateTime { get; private set; }

        public ChatUser Sender { get; private set; }

        public IPacketTarget Target { get; private set; }

        public ChatMessageFlags Flags { get; private set; } = ChatMessageFlags.Log;

        public int SequenceId { get; set; }

        public UserChannelJoinEvent(DateTimeOffset joined, ChatUser user, IPacketTarget target) {
            DateTime = joined;
            Sender = user;
            Target = target;
        }
    }
}
