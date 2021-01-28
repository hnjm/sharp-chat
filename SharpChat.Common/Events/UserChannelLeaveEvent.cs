using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public class UserChannelLeaveEvent : ChatEvent {
        public UserChannelLeaveEvent() : base() { }
        public UserChannelLeaveEvent(DateTimeOffset parted, User user, IPacketTarget target) : base(parted, user, target, ChatEventFlags.Log) {}
    }
}
