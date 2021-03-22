using SharpChat.Events;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class ChannelLeavePacket : IServerPacket {
        private ChannelLeaveEvent Leave { get; }

        public ChannelLeavePacket(ChannelLeaveEvent leave) {
            Leave = leave ?? throw new ArgumentNullException(nameof(leave));
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserMove);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerMovePacket.UserLeft);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Leave.User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Leave.EventId);

            return sb.ToString();
        }
    }
}
