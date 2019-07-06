using Fleck;
using System.Collections.Generic;
using System.Text;

namespace SquidChat
{
    public static class Extensions
    {
        public static string SanitiseMessage(this string input)
            => input.Replace(@"<", @"&lt;")
                    .Replace(@">", @"&gt;")
                    .Replace("\n", @" <br/> ")
                    .Replace("\t", @"    ");

        public static string SanitiseUsername(this string input)
            => input.Replace(' ', '_')
                    .Replace("\n", string.Empty)
                    .Replace("\r", string.Empty)
                    .Replace("\f", string.Empty)
                    .Replace("\t", string.Empty);

        public static string Pack(this IEnumerable<string> parts, SockChatClientMessage inst)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((int)inst);

            foreach (string part in parts)
            {
                sb.Append(Constants.SEPARATOR);
                sb.Append(part);
            }

            return sb.ToString();
        }

        public static void Send(this IWebSocketConnection conn, SockChatClientMessage inst, params string[] parts)
            => conn.Send(parts.Pack(inst));

        public static char AsChar(this bool b)
            => b ? '1' : '0';
    }
}
