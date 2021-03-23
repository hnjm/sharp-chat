using SharpChat.Messages;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class MessageCreateEvent : Event {
        public const string TYPE = @"message:create";

        public long MessageId { get; }
        public string Text { get; }
        public bool IsAction { get; }

        public MessageCreateEvent(IMessage message)
            : base(message.Channel, message.Sender, message.Created) {
            MessageId = message.MessageId;
            Text = message.Text;
            IsAction = message.IsAction;
        }
    }
}
