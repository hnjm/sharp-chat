using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public class BroadcastMessageEvent : Event {
        public const string TYPE = @"broadcast:message";

        public override string Type => TYPE;
        public string Text { get; }

        public BroadcastMessageEvent(ChatBot chatBot, string text)
            : base(null, chatBot, null) {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}
