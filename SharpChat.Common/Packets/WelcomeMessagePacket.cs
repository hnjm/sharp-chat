using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class WelcomeMessagePacket : ServerPacketBase {
        private const string STRING_ID = @"welcome";

        private IUser Sender { get; }
        private string Message { get; }

        public WelcomeMessagePacket(IUser sender, string message) {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ContextPopulate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerContextPacket.Message);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Sender.Pack());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(BotArguments.Notice(STRING_ID, Message));
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(STRING_ID);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append('0');
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(@"10010");

            return sb.ToString();
        }
    }
}
