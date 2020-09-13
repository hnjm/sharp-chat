using SharpChat.Packet;
using System;
using System.Text.Json.Serialization;

namespace SharpChat.Events {
    public class UserDisconnectEvent : IChatEvent {

        [JsonIgnore]
        public DateTimeOffset DateTime { get; set; }

        [JsonIgnore]
        public BasicUser Sender { get; set; }

        [JsonIgnore]
        public IPacketTarget Target { get; set; }

        [JsonIgnore]
        public string TargetName { get; set; }

        [JsonIgnore]
        public ChatMessageFlags Flags { get; set; } = ChatMessageFlags.Log;

        [JsonIgnore]
        public long SequenceId { get; set; }

        [JsonPropertyName(@"reason")]
        public UserDisconnectReason Reason { get; set; }

        public UserDisconnectEvent() { }
        public UserDisconnectEvent(DateTimeOffset parted, BasicUser user, ChatChannel target, UserDisconnectReason reason) {
            DateTime = parted;
            Sender = user;
            Target = target;
            TargetName = target?.TargetName;
            Reason = reason;
        }
    }
}
