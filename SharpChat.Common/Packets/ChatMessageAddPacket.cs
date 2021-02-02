using SharpChat.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class ChatMessageAddPacket : ServerPacketBase {
        public IMessageEvent Message { get; private set; }

        public ChatMessageAddPacket(IMessageEvent message) : base(message.EventId) {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.MessageAdd);
            sb.Append(IServerPacket.SEPARATOR);

            sb.Append(Message.DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);

            sb.Append(Message.Sender?.UserId ?? -1);
            sb.Append(IServerPacket.SEPARATOR);

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

            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(SequenceId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.AppendFormat(
                "1{0}0{1}{2}",
                Message.Flags.HasFlag(EventFlags.Action) ? '1' : '0',
                Message.Flags.HasFlag(EventFlags.Action) ? '0' : '1',
                Message.Flags.HasFlag(EventFlags.Private) ? '1' : '0'
            );
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Message.TargetName);

            yield return sb.ToString();
        }
    }
}
