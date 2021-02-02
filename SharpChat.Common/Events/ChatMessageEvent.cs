using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpChat.Events {
    public class ChatMessageEvent : ChatEvent, IMessageEvent {
        [JsonPropertyName(@"text")]
        public string Text { get; set; }

        public static string PackBotMessage(int type, string id, params string[] parts)
            => type.ToString() + '\f' + id + '\f' + string.Join('\f', parts);

        public ChatMessageEvent(IEvent evt, JsonElement elem) : base(evt, elem) {
            if(elem.TryGetProperty(@"text", out JsonElement textElem))
                Text = textElem.GetString();
        }
        public ChatMessageEvent(IUser sender, Channel target, string text, EventFlags flags = EventFlags.None, DateTimeOffset? dateTime = null)
            : base(dateTime ?? DateTimeOffset.Now, sender, target, flags) {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }

    // For existing database records
    public class ChatMessage : ChatMessageEvent {
        public ChatMessage(IEvent evt, JsonElement elem) : base(evt, elem) {}

        public ChatMessage(IUser sender, Channel target, string text, EventFlags flags = EventFlags.None, DateTimeOffset? dateTime = null)
            : base(sender, target, text, flags, dateTime) {
            throw new InvalidOperationException(@"This object only exists for database backwards compatibility, please use ChatMessageEvent instead.");
        }
    }
}
