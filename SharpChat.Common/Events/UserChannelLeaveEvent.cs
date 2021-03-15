using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Text.Json;

namespace SharpChat.Events {
    public class UserChannelLeaveEvent : Event {
        public const string TYPE = @"channel:leave";

        public override string Type => TYPE;

        private UserChannelLeaveEvent(IEvent evt) : base(evt) { }
        public UserChannelLeaveEvent(DateTimeOffset parted, IUser user, Channel target) : base(parted, user, target) {}

        public static UserChannelLeaveEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            return new UserChannelLeaveEvent(evt);
        }
    }
}
