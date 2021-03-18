using SharpChat.Users;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class UserDisconnectEvent : Event {
        public const string TYPE = @"user:disconnect";

        public override string Type => TYPE;
        public UserDisconnectReason Reason { get; }

        private UserDisconnectEvent(IEvent evt, UserDisconnectReason reason) : base(evt) {
            Reason = reason;
        }

        public UserDisconnectEvent(IEventTarget target, IUser user, UserDisconnectReason reason)
            : base(target, user) {
            Reason = reason;
        }

        public override string EncodeAsJson() {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if(Reason != UserDisconnectReason.Unknown)
                data[@"reason"] = (int)Reason;
            return JsonSerializer.Serialize(data);
        }

        public static UserDisconnectEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            UserDisconnectReason reason = UserDisconnectReason.Unknown;
            if(elem.TryGetProperty(@"reason", out JsonElement reasonElem) && reasonElem.TryGetInt32(out int intReason))
                reason = (UserDisconnectReason)intReason;

            return new UserDisconnectEvent(evt, reason);
        }
    }
}
