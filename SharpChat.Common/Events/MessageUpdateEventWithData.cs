using SharpChat.Messages;

namespace SharpChat.Events {
    public class MessageUpdateEventWithData : MessageUpdateEvent { // this is not a hack, i promise
        public IMessage Message { get; }

        public MessageUpdateEventWithData(MessageUpdateEvent mue, IMessage msg)
            : base(msg, mue.User, mue.Text) {
            Message = msg;
        }
    }
}
