using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public class UserConnectEvent : ChatEvent {
        public UserConnectEvent() : base() { }
        public UserConnectEvent(DateTimeOffset joined, BasicUser user, IPacketTarget target) : base(joined, user, target, ChatEventFlags.Log) {}
    }
}
