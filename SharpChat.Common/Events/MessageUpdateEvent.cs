using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class MessageUpdateEvent : Event, IMessageEvent, IUpdateEvent {
        public const string TYPE = @"message:update";

        public override string Type => TYPE;
        public string Text { get; }

        public long TargetId => EventId;

        public MessageUpdateEvent(IEventTarget target, IUser editor, string text)
            : base(target, editor) {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
        private MessageUpdateEvent(IEvent evt, string text) : base(evt) {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public override string EncodeAsJson() {
            return JsonSerializer.Serialize(GetUpdatedFields());
        }

        public static IEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            string text = string.Empty;
            if(elem.TryGetProperty(@"text", out JsonElement textElem))
                text = textElem.GetString();

            return new MessageUpdateEvent(evt, text);
        }

        public Dictionary<string, object> GetUpdatedFields() {
            return new Dictionary<string, object> {
                { @"text", Text },
            };
        }
    }
}
