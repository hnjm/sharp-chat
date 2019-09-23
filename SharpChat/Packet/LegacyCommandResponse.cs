using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packet
{
    public class LegacyCommandResponse : ServerPacket
    {
        public bool IsError { get; private set; }
        public string StringId { get; private set; }
        public IEnumerable<object> Arguments { get; private set; }

        public LegacyCommandResponse(
            string stringId,
            bool isError = true,
            params object[] args
        ) {
            IsError = isError;
            StringId = stringId;
            Arguments = args;
        }

        public override IEnumerable<string> Pack(int version)
        {
            if (version > 1)
                return null;

            StringBuilder sb = new StringBuilder();

            if(StringId == LCR.WELCOME)
            {
                sb.Append((int)SockChatServerPacket.ContextPopulate);
                sb.Append('\t');
                sb.Append((int)SockChatServerContextPacket.Message);
                sb.Append('\t');
                sb.Append(DateTimeOffset.Now.ToSockChatSeconds(version));
                sb.Append("\t-1\tChatBot\tinherit\t\t");
            } else
            {
                sb.Append((int)SockChatServerPacket.MessageAdd);
                sb.Append('\t');
                sb.Append(DateTimeOffset.Now.ToSockChatSeconds(version));
                sb.Append("\t-1\t");
            }

            sb.Append(IsError ? '1' : '0');
            sb.Append('\f');
            sb.Append(StringId == LCR.WELCOME ? LCR.BROADCAST : StringId);

            if(Arguments?.Any() == true)
                lock(Arguments)
                    foreach(object arg in Arguments) {
                        sb.Append('\f');
                        sb.Append(arg);
                    }

            sb.Append('\t');

            if(StringId == LCR.WELCOME) {
                sb.Append(StringId);
                sb.Append("\t0");
            } else
                sb.Append(SequenceId);

            sb.Append("\t10010");
            /*sb.AppendFormat(
                "\t1{0}0{1}{2}",
                Flags.HasFlag(ChatMessageFlags.Action) ? '1' : '0',
                Flags.HasFlag(ChatMessageFlags.Action) ? '0' : '1',
                Flags.HasFlag(ChatMessageFlags.Private) ? '1' : '0'
            );*/

            return new[] { sb.ToString() };
        }
    }

    // Abbreviated class name because otherwise shit gets wide
    public static class LCR
    {
        public const string COMMAND_NOT_FOUND = @"nocmd";
        public const string COMMAND_NOT_ALLOWED = @"cmdna";
        public const string COMMAND_FORMAT_ERROR = @"cmderr";
        public const string WELCOME = @"welcome";
        public const string BROADCAST = @"say";
        public const string IP_ADDRESS = @"ipaddr";
        public const string USER_NOT_FOUND = @"usernf";
        public const string UNSILENCED = @"unsil";
        public const string TARGET_UNSILENCED = @"usilok";
        public const string TARGET_NOT_SILENCED = @"usilerr";
        public const string TARGET_SILENCE_NOT_ALLOWED = @"usilperr";
        public const string NAME_IN_USE = @"nameinuse";
    }
}
