using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class ChatMessageAddPacket : ServerPacketBase {
        protected DateTimeOffset DateTime { get; }
        protected IUser Sender { get; }
        protected string TargetName { get; }
        protected string Text { get; set; }
        protected bool IsAction { get; }

        public ChatMessageAddPacket(IUser sender, string text, bool isAction = false) {
            DateTime = DateTimeOffset.Now;
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IsAction = isAction;
        }

        public ChatMessageAddPacket(MessageCreateEvent msg)
            : this(msg.EventId, msg.DateTime, msg.Sender, msg.Text, msg.Target, msg.IsAction) { }

        public ChatMessageAddPacket(
            long eventId,
            DateTimeOffset dateTime,
            IUser sender,
            string text,
            string target,
            bool isAction = false
        ) : base(eventId) {
            if(text == null)
                throw new ArgumentNullException(nameof(text));

            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            TargetName = target ?? throw new ArgumentNullException(nameof(target));
            DateTime = dateTime;
            IsAction = isAction;

            StringBuilder sb = new StringBuilder();

            if(isAction)
                sb.Append(@"<i>");

            sb.Append(
                text.Replace(@"<", @"&lt;")
                    .Replace(@">", @"&gt;")
                    .Replace("\n", @" <br/> ")
                    .Replace("\t", @"    ")
            );

            if(isAction)
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
                IsAction ? '1' : '0',
                IsAction ? '0' : '1',
                /*Flags.HasFlag(EventFlags.Private)*/ false ? '1' : '0'
            );
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(TargetName);

            return sb.ToString();
        }
    }
}
