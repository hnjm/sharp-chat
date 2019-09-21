using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class ChatMessageAddPacket : ServerPacket
    {
        public IChatMessage Message { get; private set; }

        public ChatMessageAddPacket(IChatMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public override IEnumerable<string> Pack(int version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.MessageAdd);
            sb.Append('\t');

            if(version >= 2)
            {
                sb.Append(Message.Channel?.Name ?? @"@broadcast");
                sb.Append('\t');
            }

            sb.Append(Message.DateTime.ToSockChatSeconds(version));
            sb.Append('\t');

            sb.Append(Message.User?.UserId ?? -1);
            sb.Append('\t');

            if (version >= 2)
                sb.Append(Message.Text);
            else
            {
                if (Message.Flags == SockChatMessageFlags.Action)
                    sb.Append(@"<i>");

                sb.Append(
                    Message.Text
                        .Replace(@"<", @"&lt;")
                        .Replace(@">", @"&gt;")
                        .Replace("\n", @" <br/> ")
                        .Replace("\t", @"    ")
                );

                if (Message.Flags == SockChatMessageFlags.Action)
                    sb.Append(@"</i>");
            }

            sb.Append('\t');
            sb.Append(SequenceId);
            sb.Append('\t');
            sb.Append(Message.Flags.Serialise());

            return new[] { sb.ToString() };
        }
    }
}
