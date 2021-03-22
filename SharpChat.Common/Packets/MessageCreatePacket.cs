using SharpChat.Events;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class MessageCreatePacket : IServerPacket {
        private MessageCreateEvent Create { get; }
        private string FinalText { get; }

        public MessageCreatePacket(MessageCreateEvent create) {
            Create = create ?? throw new ArgumentNullException(nameof(create));

            StringBuilder sb = new StringBuilder();

            if(create.IsAction)
                sb.Append(@"<i>");

            sb.Append(
                create.Text
                    .Replace(@"<", @"&lt;")
                    .Replace(@">", @"&gt;")
                    .Replace("\n", @" <br/> ")
                    .Replace("\t", @"    ")
            );

            if(create.IsAction)
                sb.Append(@"</i>");

            FinalText = sb.ToString();
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.MessageAdd);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Create.DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Create.User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(FinalText);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Create.EventId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.AppendFormat(
                "1{0}0{1}{2}",
                Create.IsAction ? '1' : '0',
                Create.IsAction ? '0' : '1',
                /*Flags.HasFlag(EventFlags.Private)*/ false ? '1' : '0'
            );
            sb.Append(IServerPacket.SEPARATOR);
            if(!Create.IsBroadcast())
                sb.Append(Create.Channel.Name);

            return sb.ToString();
        }
    }
}
