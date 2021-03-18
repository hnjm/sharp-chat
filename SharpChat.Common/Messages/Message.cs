using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Messages {
    public class Message {
        public long MessageId { get; }
        public IChannel Channel { get; }
        public IUser Sender { get; }
        public string Text { get; }
        public bool IsAction { get; }
        public DateTimeOffset Created { get; }
        public DateTimeOffset? Edited { get; }

        public bool IsEdited => Edited.HasValue;

        public Message(
            IChannel channel,
            IUser sender,
            string text,
            bool isAction = false,
            DateTimeOffset? created = null,
            DateTimeOffset? edited = null
        ) {
            MessageId = SharpId.Next();
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IsAction = isAction;
            Created = created ?? DateTimeOffset.Now;
            Edited = edited;
        }
    }
}
