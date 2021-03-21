using SharpChat.Messages;
using SharpChat.Users;

namespace SharpChat.Events {
    public class MessageDeleteEvent : Event {
        public const string TYPE = @"message:delete";

        public override string Type => TYPE;
        public long MessageId { get; }

        public MessageDeleteEvent(IUser actor, IMessage message)
            : base(message.Channel, actor) {
            MessageId = message.MessageId;
        }
    }
}
