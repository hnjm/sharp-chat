using SharpChat.Users;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class UserDisconnectEvent : Event {
        public const string TYPE = @"user:disconnect";

        public override string Type => TYPE;
        public UserDisconnectReason Reason { get; }

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
    }
}
