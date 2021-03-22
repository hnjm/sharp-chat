using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class ChannelJoinPacket : IServerPacket {
        private ChannelJoinEvent Join { get; }

        public ChannelJoinPacket(ChannelJoinEvent join) {
            Join = join ?? throw new ArgumentNullException(nameof(join));
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserMove);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerMovePacket.UserJoined);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Join.User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Join.User.GetDisplayName());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Join.User.Colour);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Join.EventId);

            return sb.ToString();
        }
    }
}
