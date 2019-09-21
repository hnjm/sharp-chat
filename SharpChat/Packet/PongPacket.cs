using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class PongPacket : ServerPacket
    {
        public DateTimeOffset PongTime { get; private set; }

        public PongPacket(DateTimeOffset dto)
        {
            PongTime = dto;
        }

        public override IEnumerable<string> Pack(int version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.Pong);
            sb.Append('\t');

            if (version >= 2)
                sb.Append(PongTime.ToSockChatSeconds(version));
            else
                sb.Append(@"pong");

            return new[] { sb.ToString() };
        }
    }
}
