using System;
using System.Text;

namespace SharpChat.Packet
{
    public class PongPacket : IServerPacket
    {
        public DateTimeOffset PongTime { get; private set; }

        public PongPacket(DateTimeOffset dto)
        {
            PongTime = dto;
        }

        public string Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.Pong);
            sb.Append(Constants.SEPARATOR);

            if (version >= 2)
                sb.Append(PongTime.ToUnixTimeSeconds());
            else
                sb.Append(@"pong");

            return sb.ToString();
        }
    }
}
