using SharpChat.Channels;
using SharpChat.Users;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class ChannelLeaveEvent : Event {
        public const string TYPE = @"channel:leave";

        public override string Type => TYPE;
        public UserDisconnectReason Reason { get; }

        public ChannelLeaveEvent(IChannel channel, IUser user, UserDisconnectReason reason)
            : base(channel, user) {
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
