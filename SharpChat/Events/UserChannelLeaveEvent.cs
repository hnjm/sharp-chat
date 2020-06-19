using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Events {
    public class UserChannelLeaveEvent : IChatEvent {
        public DateTimeOffset DateTime { get; private set; }

        public ChatUser Sender { get; private set; }

        public IPacketTarget Target { get; private set; }

        public ChatMessageFlags Flags { get; private set; } = ChatMessageFlags.Log;

        public long SequenceId { get; set; }

        public UserChannelLeaveEvent(DateTimeOffset parted, ChatUser user, IPacketTarget target) {
            DateTime = parted;
            Sender = user;
            Target = target;
        }
    }
}
