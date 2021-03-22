using SharpChat.Events;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class MessageDeletePacket : IServerPacket {
        public long MessageId { get; }

        public MessageDeletePacket(MessageUpdateEvent mue) {
            MessageId = (mue ?? throw new ArgumentNullException(nameof(mue))).MessageId;
        }

        public MessageDeletePacket(MessageDeleteEvent mde) {
            MessageId = (mde ?? throw new ArgumentNullException(nameof(mde))).MessageId;
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.MessageDelete);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(MessageId);

            return sb.ToString();
        }
    }
}
