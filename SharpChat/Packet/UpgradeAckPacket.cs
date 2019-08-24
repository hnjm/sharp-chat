using System.Text;

namespace SharpChat.Packet
{
    public class UpgradeAckPacket : IServerPacket
    {
        public bool Success { get; private set; }
        public int Version { get; private set; }

        public UpgradeAckPacket(bool success, int version)
        {
            Success = success;
            Version = version;
        }

        public string Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UpgradeAck);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Success ? '1' : '0');
            sb.Append(Constants.SEPARATOR);
            sb.Append(Version);

            return sb.ToString();
        }
    }
}
