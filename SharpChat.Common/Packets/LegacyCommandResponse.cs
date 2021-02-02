using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packets {
    public class LegacyCommandResponse : ServerPacketBase {
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

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            if (StringId == LCR.WELCOME) {
                sb.Append((int)ServerPacket.ContextPopulate);
                sb.Append('\t');
                sb.Append((int)ServerContextPacket.Message);
                sb.Append('\t');
                sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
                sb.Append('\t');
                sb.Append("-1\tChatBot\tinherit\t"); // HERE
                sb.Append('\t');
            } else {
                sb.Append((int)ServerPacket.MessageAdd);
                sb.Append('\t');
                sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
                sb.Append('\t');
                sb.Append(-1); // HERE
                sb.Append('\t');
            }

            sb.Append(IsError ? '1' : '0');
            sb.Append('\f');
            sb.Append(StringId == LCR.WELCOME ? LCR.BROADCAST : StringId);

            if (Arguments?.Any() == true)
                foreach (object arg in Arguments) {
                    sb.Append('\f');
                    sb.Append(arg);
                }

            sb.Append('\t');

            if (StringId == LCR.WELCOME) {
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

            yield return sb.ToString();
        }
    }

    // Abbreviated class name because otherwise shit gets wide
    public static class LCR {
        public const string COMMAND_NOT_FOUND = @"nocmd";
        public const string COMMAND_NOT_ALLOWED = @"cmdna";
        public const string COMMAND_FORMAT_ERROR = @"cmderr";
        public const string WELCOME = @"welcome";
        public const string BROADCAST = @"say";
        public const string IP_ADDRESS = @"ipaddr";
        public const string USER_NOT_FOUND = @"usernf";
        public const string SILENCE_SELF = @"silself";
        public const string SILENCE_RANK = @"silperr";
        public const string SILENCE_ALREADY = @"silerr";
        public const string TARGET_SILENCED = @"silok";
        public const string SILENCED = @"silence";
        public const string UNSILENCED = @"unsil";
        public const string TARGET_UNSILENCED = @"usilok";
        public const string NOT_SILENCED = @"usilerr";
        public const string UNSILENCE_RANK = @"usilperr";
        public const string NAME_IN_USE = @"nameinuse";
        public const string CHANNEL_INSUFFICIENT_HIERARCHY = @"ipchan";
        public const string CHANNEL_INVALID_PASSWORD = @"ipwchan";
        public const string CHANNEL_NOT_FOUND = @"nochan";
        public const string CHANNEL_ALREADY_EXISTS = @"nischan";
        public const string CHANNEL_NAME_INVALID = "inchan";
        public const string CHANNEL_CREATED = @"crchan";
        public const string CHANNEL_DELETE_FAILED = @"ndchan";
        public const string CHANNEL_DELETED = @"delchan";
        public const string CHANNEL_PASSWORD_CHANGED = @"cpwdchan";
        public const string CHANNEL_HIERARCHY_CHANGED = @"cprivchan";
        public const string USERS_LISTING_ERROR = @"whoerr";
        public const string USERS_LISTING_CHANNEL = @"whochan";
        public const string USERS_LISTING_SERVER = @"who";
        public const string INSUFFICIENT_RANK = @"rankerr";
        public const string MESSAGE_DELETE_ERROR = @"delerr";
        public const string KICK_NOT_ALLOWED = @"kickna";
        public const string USER_NOT_BANNED = @"notban";
        public const string USER_UNBANNED = @"unban";
    }
}
