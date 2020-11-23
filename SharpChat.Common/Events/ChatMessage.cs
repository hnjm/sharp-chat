using SharpChat.Users;
using System;
using System.Text.Json.Serialization;

namespace SharpChat.Events {
    public class ChatMessage : IChatMessage {
        [JsonIgnore]
        public BasicUser Sender { get; set; }

        [JsonIgnore]
        public IPacketTarget Target { get; set; }

        [JsonIgnore]
        public string TargetName { get; set; }

        [JsonIgnore]
        public DateTimeOffset DateTime { get; set; }

        [JsonIgnore]
        public ChatMessageFlags Flags { get; set; } = ChatMessageFlags.None;

        [JsonIgnore]
        public long SequenceId { get; set; }

        [JsonPropertyName(@"text")]
        public string Text { get; set; }

        public static string PackBotMessage(int type, string id, params string[] parts) {
            return type.ToString() + '\f' + id + '\f' + string.Join('\f', parts);
        }
    }
}
