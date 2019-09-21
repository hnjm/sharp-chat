using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class ContextMessagePacket : ServerPacket
    {
        public IChatMessage Message { get; private set; }
        public bool Notify { get; private set; }

        public ContextMessagePacket(IChatMessage message, bool notify = false)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Notify = notify;
        }

        public override IEnumerable<string> Pack(int version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ContextPopulate);
            sb.Append('\t');
            sb.Append((int)SockChatServerContextPacket.Message);
            sb.Append('\t');
            sb.Append(Message.DateTime.ToUnixTimeSeconds());
            sb.Append('\t');

            sb.Append(Message.User.UserId);
            sb.Append('\t');
            sb.Append(Message.User.Username);
            sb.Append('\t');

            if(version >= 2)
                sb.Append(Message.User.Colour.Raw);
            else
                sb.Append(Message.User.Colour);

            sb.Append('\t');

            sb.Append(Message.User.Hierarchy);
            sb.Append(' ');
            sb.Append(Message.User.IsModerator ? '1' : '0');
            sb.Append(@" 0 ");
            sb.Append(Message.User.CanChangeNick ? '1' : '0');
            sb.Append(' ');
            sb.Append((int)Message.User.CanCreateChannels);

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

            if(Message is EventChatMessage ecm && !string.IsNullOrEmpty(ecm.MessageIdStr))
                sb.Append(ecm.MessageIdStr);
            else
                sb.Append(SequenceId);

            sb.Append('\t');
            sb.Append(Notify ? '1' : '0');
            sb.Append('\t');
            sb.Append(Message.Flags.Serialise());

            return new[] { sb.ToString() };
        }
    }
}
