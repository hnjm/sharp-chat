using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class MessageCreateEvent : Event, IMessageEvent {
        public const string TYPE = @"message:create";

        public override string Type => TYPE;
        public string Text { get; }
        public bool IsAction { get; }

        public MessageCreateEvent(IUser sender, Channel target, string text, bool isAction = false, DateTimeOffset? dateTime = null)
            : base(dateTime ?? DateTimeOffset.Now, sender, target) {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IsAction = isAction;
        }
        private MessageCreateEvent(IEvent evt, string text, bool isAction = false) : base(evt) {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IsAction = isAction;
        }

        public override string EncodeAsJson() {
            Dictionary<string, object> data = new Dictionary<string, object> {
                { @"text", Text },
            };
            if(IsAction)
                data[@"action"] = true;
            return JsonSerializer.Serialize(data);
        }

        public static IEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            string text = string.Empty;
            if(elem.TryGetProperty(@"text", out JsonElement textElem))
                text = textElem.GetString();

            bool isAction = false;
            if(elem.TryGetProperty(@"action", out JsonElement actionElem))
                isAction = actionElem.GetBoolean();

            return new MessageCreateEvent(evt, text, isAction);
        }
    }
}
