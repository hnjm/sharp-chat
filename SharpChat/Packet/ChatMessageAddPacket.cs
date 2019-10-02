using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet {
    public class ChatMessageAddPacket : ServerPacket {
        public IChatMessage Message { get; private set; }

        public ChatMessageAddPacket(IChatMessage message) : base(message?.SequenceId ?? 0) {
            Message = message ?? throw new ArgumentNullException(nameof(message));

            if (Message.SequenceId < 1)
                Message.SequenceId = SequenceId;
        }

        public override IEnumerable<string> Pack(int version) {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.MessageAdd);
            sb.Append('\t');

            if (version >= 2) {
                sb.Append(Message.Target.TargetName);
                sb.Append('\t');
            }

            sb.Append(Message.DateTime.ToSockChatSeconds(version));
            sb.Append('\t');

            sb.Append(Message.Sender?.UserId ?? -1);
            sb.Append('\t');

            if (version >= 2)
                sb.Append(Message.Text);
            else {
                if (Message.Flags.HasFlag(ChatMessageFlags.Action))
                    sb.Append(@"<i>");

                sb.Append(
                    Message.Text
                        .Replace(@"<", @"&lt;")
                        .Replace(@">", @"&gt;")
                        .Replace("\n", @" <br/> ")
                        .Replace("\t", @"    ")
                );

                if (Message.Flags.HasFlag(ChatMessageFlags.Action))
                    sb.Append(@"</i>");
            }

            sb.Append('\t');
            sb.Append(SequenceId);

            if (version >= 2) {
                sb.Append('\t');
                sb.Append((int)Message.Flags);
            } else {
                sb.AppendFormat(
                    "\t1{0}0{1}{2}",
                    Message.Flags.HasFlag(ChatMessageFlags.Action) ? '1' : '0',
                    Message.Flags.HasFlag(ChatMessageFlags.Action) ? '0' : '1',
                    Message.Flags.HasFlag(ChatMessageFlags.Private) ? '1' : '0'
                );
            }

            return new[] { sb.ToString() };
        }
    }
}
