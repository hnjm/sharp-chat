using System;
using System.Text;

namespace SharpChat.Packet
{
    public class ContextMessagePacket : IServerPacket
    {
        public IChatMessage Message { get; private set; }
        public bool Notify { get; private set; }

        public ContextMessagePacket(IChatMessage message, bool notify = false)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Notify = notify;
        }

        public string Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ContextPopulate);
            sb.Append(Constants.SEPARATOR);
            sb.Append((int)SockChatServerContextPacket.Message);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Message.DateTime.ToUnixTimeSeconds());
            sb.Append(Constants.SEPARATOR);

            sb.Append(Message.User.UserId);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Message.User.Username);
            sb.Append(Constants.SEPARATOR);

            if(version >= 2)
                sb.Append(Message.User.Colour.Raw);
            else
                sb.Append(Message.User.Colour);

            sb.Append(Constants.SEPARATOR);

            sb.Append(Message.User.Hierarchy);
            sb.Append(' ');
            sb.Append(Message.User.IsModerator.AsChar());
            sb.Append(@" 0 ");
            sb.Append(Message.User.CanChangeNick.AsChar());
            sb.Append(' ');
            sb.Append((int)Message.User.CanCreateChannels);

            sb.Append(Constants.SEPARATOR);

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

            sb.Append(Constants.SEPARATOR);

            if(Message is EventChatMessage ecm && !string.IsNullOrEmpty(ecm.MessageIdStr))
                sb.Append(ecm.MessageIdStr);
            else
                sb.Append(eventId);

            sb.Append(Constants.SEPARATOR);
            sb.Append(Notify ? '1' : '0');
            sb.Append(Constants.SEPARATOR);
            sb.Append(Message.Flags.Serialise());

            return sb.ToString();
        }
    }
}
