using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpChat.Events {
    public class UserDisconnectEvent : ChatEvent {
        [JsonPropertyName(@"reason")]
        public UserDisconnectReason Reason { get; set; }

        public UserDisconnectEvent(IEvent evt, JsonElement elem) : base(evt, elem) {
            if(elem.TryGetProperty(@"reason", out JsonElement reasonElem) && reasonElem.TryGetInt32(out int reason))
                Reason = (UserDisconnectReason)reason;
            else
                Reason = UserDisconnectReason.Unknown;
        }

        public UserDisconnectEvent(DateTimeOffset parted, IUser user, IPacketTarget target, UserDisconnectReason reason)
            : base(parted, user, target, EventFlags.Log) {
            Reason = reason;
        }
    }
}
