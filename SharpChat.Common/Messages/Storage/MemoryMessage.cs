using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Messages.Storage {
    public class MemoryMessage : IMessage, IEventHandler {
        public long MessageId { get; }
        public IChannel Channel { get; }
        public IUser Sender { get; }
        public string Text { get; private set; }
        public bool IsAction { get; }
        public DateTimeOffset Created { get; }
        public DateTimeOffset? Edited { get; private set; }
        public bool IsEdited => Edited.HasValue;

        public MemoryMessage(MemoryMessageChannel channel, MessageCreateEvent mce) {
            if(mce == null)
                throw new ArgumentNullException(nameof(mce));
            MessageId = mce.MessageId;
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Sender = new MemoryMessageUser(mce);
            Text = mce.Text;
            IsAction = mce.IsAction;
            Created = mce.DateTime;
        }

        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case MessageUpdateEvent mue:
                    Edited = mue.DateTime;
                    if(mue.HasText)
                        Text = mue.Text;
                    break;
            }
        }
    }
}
