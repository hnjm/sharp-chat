using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet {
    public class ContextMessagePacket : ServerPacket {
        public IChatMessage Message { get; private set; }
        public bool Notify { get; private set; }

        public ContextMessagePacket(IChatMessage message, bool notify = false) {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Notify = notify;
        }

        public override IEnumerable<string> Pack(int version) {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ContextPopulate);
            sb.Append('\t');
            sb.Append((int)SockChatServerContextPacket.Message);
            sb.Append('\t');
            sb.Append(Message.DateTime.ToSockChatSeconds(version));
            sb.Append('\t');
            sb.Append(Message.Sender.Pack(version));
            sb.Append('\t');

            if (version >= 2)
                sb.Append(Message.Text);
            else
                sb.Append(
                    Message.Text
                        .Replace(@"<", @"&lt;")
                        .Replace(@">", @"&gt;")
                        .Replace("\n", @" <br/> ")
                        .Replace("\t", @"    ")
                );

            sb.Append('\t');
            sb.Append(Message.SequenceId);
            sb.Append('\t');
            sb.Append(Notify ? '1' : '0');

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
