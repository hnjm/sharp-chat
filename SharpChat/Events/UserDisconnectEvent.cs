using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Events {
    public class UserDisconnectEvent : IChatEvent {
        public DateTimeOffset DateTime { get; private set; }

        public ChatUser Sender { get; private set; }

        public IPacketTarget Target { get; private set; }

        public ChatMessageFlags Flags { get; private set; } = ChatMessageFlags.Log;

        public int SequenceId { get; set; }

        public UserDisconnectReason Reason { get; private set; }

        public UserDisconnectEvent(DateTimeOffset parted, ChatUser user, IPacketTarget target, UserDisconnectReason reason) {
            DateTime = parted;
            Sender = user;
            Target = target;
            Reason = reason;
        }
    }
}
