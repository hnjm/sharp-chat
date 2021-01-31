using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Text.Json.Serialization;

namespace SharpChat.Events {
    public class UserDisconnectEvent : ChatEvent {
        [JsonPropertyName(@"reason")]
        public UserDisconnectReason Reason { get; set; }

        public UserDisconnectEvent() : base() {}
        public UserDisconnectEvent(DateTimeOffset parted, IUser user, IPacketTarget target, UserDisconnectReason reason)
            : base(parted, user, target, EventFlags.Log) {
            Reason = reason;
        }
    }
}
