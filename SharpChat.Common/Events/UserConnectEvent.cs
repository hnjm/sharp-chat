using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public class UserConnectEvent : ChatEvent {
        public UserConnectEvent() : base() { }
        public UserConnectEvent(DateTimeOffset joined, IUser user, IPacketTarget target) : base(joined, user, target, EventFlags.Log) {}
    }
}
