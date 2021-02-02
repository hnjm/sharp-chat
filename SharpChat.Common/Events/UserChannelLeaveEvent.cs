using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Text.Json;

namespace SharpChat.Events {
    public class UserChannelLeaveEvent : ChatEvent {
        public UserChannelLeaveEvent(IEvent evt, JsonElement elem) : base(evt, elem) { }
        public UserChannelLeaveEvent(DateTimeOffset parted, IUser user, Channel target) : base(parted, user, target, EventFlags.Log) {}
    }
}
