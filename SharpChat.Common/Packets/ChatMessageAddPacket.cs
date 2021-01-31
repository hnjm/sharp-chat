using SharpChat.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class ChatMessageAddPacket : ServerPacket {
        public IMessageEvent Message { get; private set; }

        public ChatMessageAddPacket(IMessageEvent message) : base(message?.SequenceId ?? 0) {
            Message = message ?? throw new ArgumentNullException(nameof(message));

            if (Message.SequenceId < 1)
                Message.SequenceId = SequenceId;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.MessageAdd);
            sb.Append('\t');

            sb.Append(Message.DateTime.ToUnixTimeSeconds());
            sb.Append('\t');

            sb.Append(Message.Sender?.UserId ?? -1);
            sb.Append('\t');

            if (Message.Flags.HasFlag(EventFlags.Action))
                sb.Append(@"<i>");

            sb.Append(
                Message.Text
                    .Replace(@"<", @"&lt;")
                    .Replace(@">", @"&gt;")
                    .Replace("\n", @" <br/> ")
                    .Replace("\t", @"    ")
            );

            if (Message.Flags.HasFlag(EventFlags.Action))
                sb.Append(@"</i>");

            sb.Append('\t');
            sb.Append(SequenceId);
            sb.AppendFormat(
                "\t1{0}0{1}{2}",
                Message.Flags.HasFlag(EventFlags.Action) ? '1' : '0',
                Message.Flags.HasFlag(EventFlags.Action) ? '0' : '1',
                Message.Flags.HasFlag(EventFlags.Private) ? '1' : '0'
            );
            sb.Append('\t');
            sb.Append(Message.TargetName);

            yield return sb.ToString();
        }
    }
}
