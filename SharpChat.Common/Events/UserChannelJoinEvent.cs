using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Text.Json;

namespace SharpChat.Events {
    public class UserChannelJoinEvent : Event {
        public const string TYPE = @"channel:join";

        public override string Type => TYPE;

        private UserChannelJoinEvent(IEvent evt) : base(evt) {}
        public UserChannelJoinEvent(DateTimeOffset joined, IUser user, Channel target) : base(joined, user, target) {}

        public static UserChannelJoinEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            return new UserChannelJoinEvent(evt);
        }
    }
}
