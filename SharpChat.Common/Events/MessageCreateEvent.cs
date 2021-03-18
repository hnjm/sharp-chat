using SharpChat.Messages;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class MessageCreateEvent : Event {
        public const string TYPE = @"message:create";

        public override string Type => TYPE;
        public long MessageId { get; }
        public string Text { get; }
        public bool IsAction { get; }

        public MessageCreateEvent(Message message)
            : this(message.Channel, message.Sender, message.MessageId, message.Text, message.IsAction) { }
        public MessageCreateEvent(IEventTarget target, IUser sender, long messageId, string text, bool isAction = false)
            : base(target, sender) {
            MessageId = messageId;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IsAction = isAction;
        }
        private MessageCreateEvent(IEvent evt, long messageId, string text, bool isAction) : base(evt) {
            MessageId = messageId;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IsAction = isAction;
        }

        public override string EncodeAsJson() {
            Dictionary<string, object> data = new Dictionary<string, object> {
                { @"id", MessageId },
                { @"text", Text },
            };
            if(IsAction)
                data[@"action"] = true;
            return JsonSerializer.Serialize(data);
        }

        public static IEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            long messageId = evt.EventId;
            if(elem.TryGetProperty(@"id", out JsonElement idElem) && !idElem.TryGetInt64(out messageId))
                messageId = evt.EventId;

            string text = string.Empty;
            if(elem.TryGetProperty(@"text", out JsonElement textElem))
                text = textElem.GetString();

            bool isAction = false;
            if(elem.TryGetProperty(@"action", out JsonElement actionElem))
                isAction = actionElem.GetBoolean();

            return new MessageCreateEvent(evt, messageId, text, isAction);
        }
    }
}
