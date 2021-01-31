using SharpChat.Users;
using System;
using System.Text.Json;

namespace SharpChat.Events {
    public class UserChannelJoinEvent : ChatEvent {
        public UserChannelJoinEvent(IEvent evt, JsonElement elem) : base(evt, elem) {}
        public UserChannelJoinEvent(DateTimeOffset joined, IUser user, IPacketTarget target) : base(joined, user, target, EventFlags.Log) {}
    }
}
