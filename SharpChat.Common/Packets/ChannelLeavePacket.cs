using SharpChat.Events;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class ChannelLeavePacket : ServerPacket {
        private ChannelLeaveEvent Leave { get; }

        public ChannelLeavePacket(ChannelLeaveEvent leave) {
            Leave = leave ?? throw new ArgumentNullException(nameof(leave));
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.UserMove);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerMoveSubPacketId.UserLeft);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Leave.User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Leave.EventId);

            return sb.ToString();
        }
    }
}
