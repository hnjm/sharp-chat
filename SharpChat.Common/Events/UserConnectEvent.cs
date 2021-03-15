using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Text.Json;

namespace SharpChat.Events {
    public class UserConnectEvent : Event {
        public const string TYPE = @"user:connect";

        public override string Type => TYPE;

        private UserConnectEvent(IEvent evt) : base(evt) { }
        public UserConnectEvent(DateTimeOffset joined, IUser user, Channel target) : base(joined, user, target) {}

        public static UserConnectEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            return new UserConnectEvent(evt);
        }
    }
}
