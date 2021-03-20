using SharpChat.Channels;
using SharpChat.Database;
using SharpChat.Users;
using System;

namespace SharpChat.Messages.Storage {
    public class ADOMessage : IMessage {
        public long MessageId { get; }
        public IChannel Channel { get; }
        public IUser Sender { get; }
        public string Text { get; }
        public DateTimeOffset Created { get; }
        public DateTimeOffset? Edited { get; }

        public bool IsAction => (Flags & IS_ACTION) == IS_ACTION;
        public bool IsEdited => Edited.HasValue;

        public const byte IS_ACTION = 1;
        public byte Flags { get; }

        public ADOMessage(IDatabaseReader reader) {
            if(reader == null)
                throw new ArgumentNullException(nameof(reader));
            MessageId = reader.ReadI64(@"msg_id");
            Channel = new ADOMessageChannel(reader);
            Sender = new ADOMessageUser(reader);
            Text = reader.ReadString(@"msg_text");
            Flags = reader.ReadU8(@"msg_flags");
            Created = DateTimeOffset.FromUnixTimeSeconds(reader.ReadI64(@"msg_created"));
            Edited = reader.IsNull(@"msg_edited") ? null : DateTimeOffset.FromUnixTimeSeconds(reader.ReadI64(@"msg_edited"));
        }
    }
}
