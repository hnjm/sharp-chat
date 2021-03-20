using SharpChat.Users;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class UserConnectEvent : Event {
        public const string TYPE = @"user:connect";

        public override string Type => TYPE;
        public UserStatus Status { get; }
        public string StatusMessage { get; }

        public UserConnectEvent(IEventTarget target, IUser user) : base(target, user) {
            Status = user.Status;
            StatusMessage = user.StatusMessage;
        }

        public override string EncodeAsJson() {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if(Status != UserStatus.Unknown)
                data[@"s"] = (int)Status;
            if(!string.IsNullOrWhiteSpace(StatusMessage))
                data[@"sm"] = StatusMessage;
            return JsonSerializer.Serialize(data);
        }
    }
}
