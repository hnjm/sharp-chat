using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public class UserConnectEvent : ChatEvent {
        public UserConnectEvent() : base() { }
        public UserConnectEvent(DateTimeOffset joined, User user, IPacketTarget target) : base(joined, user, target, ChatEventFlags.Log) {}
    }
}
