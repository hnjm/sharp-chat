using SharpChat.Messages;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class ChatMessageDeletePacket : ServerPacketBase {
        public IMessage Message { get; private set; }

        public ChatMessageDeletePacket(IMessage msg) {
            Message = msg ?? throw new ArgumentNullException(nameof(msg));
        }

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.MessageDelete);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Message.MessageId);

            return sb.ToString();
        }
    }
}
