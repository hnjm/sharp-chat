using SharpChat.Users;
using System;
using System.Text.Json.Serialization;

namespace SharpChat.Events {
    public class ChatMessageEvent : ChatEvent, IMessageEvent {
        [JsonPropertyName(@"text")]
        public string Text { get; set; }

        public static string PackBotMessage(int type, string id, params string[] parts)
            => type.ToString() + '\f' + id + '\f' + string.Join('\f', parts);

        public ChatMessageEvent() : base() {}
        public ChatMessageEvent(IUser sender, IPacketTarget target, string text, EventFlags flags = EventFlags.None, DateTimeOffset? dateTime = null)
            : base(dateTime ?? DateTimeOffset.Now, sender, target, flags) {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }

    // For existing database records
    public class ChatMessage : ChatMessageEvent { }
}
