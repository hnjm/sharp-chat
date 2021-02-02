using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class ChatMessageAddPacket : ServerPacketBase {
        protected DateTimeOffset DateTime { get; }
        protected IUser Sender { get; }
        protected EventFlags Flags { get; }
        protected Channel Target { get; }
        protected string Text { get; set; }

        public ChatMessageAddPacket(IUser sender, string text) {
            DateTime = DateTimeOffset.Now;
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public ChatMessageAddPacket(IMessageEvent msg)
            : this(msg.EventId, msg.DateTime, msg.Sender, msg.Text, msg.Flags, msg.Target) { }

        public ChatMessageAddPacket(
            long eventId,
            DateTimeOffset dateTime,
            IUser sender,
            string text,
            EventFlags flags = EventFlags.None,
            Channel target = null
        ) : base(eventId) {
            if(text == null)
                throw new ArgumentNullException(nameof(text));

            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            DateTime = dateTime;
            Flags = flags;
            Target = target;

            StringBuilder sb = new StringBuilder();

            if(Flags.HasFlag(EventFlags.Action))
                sb.Append(@"<i>");

            sb.Append(
                text.Replace(@"<", @"&lt;")
                    .Replace(@">", @"&gt;")
                    .Replace("\n", @" <br/> ")
                    .Replace("\t", @"    ")
            );

            if(Flags.HasFlag(EventFlags.Action))
                sb.Append(@"</i>");

            Text = sb.ToString();
        }

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.MessageAdd);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Sender.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Text);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(SequenceId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.AppendFormat(
                "1{0}0{1}{2}",
                Flags.HasFlag(EventFlags.Action) ? '1' : '0',
                Flags.HasFlag(EventFlags.Action) ? '0' : '1',
                Flags.HasFlag(EventFlags.Private) ? '1' : '0'
            );
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Target?.Name ?? string.Empty);

            return sb.ToString();
        }
    }
}
