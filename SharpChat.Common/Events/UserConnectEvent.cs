using SharpChat.Users;
using System.Text.Json;

namespace SharpChat.Events {
    public class UserConnectEvent : Event {
        public const string TYPE = @"user:connect";

        public override string Type => TYPE;

        private UserConnectEvent(IEvent evt) : base(evt) { }
        public UserConnectEvent(IEventTarget target, IUser user) : base(target, user) {}

        public static UserConnectEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            return new UserConnectEvent(evt);
        }
    }
}
