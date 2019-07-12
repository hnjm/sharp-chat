using Fleck;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SharpChat
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

        public static string Pack(this IEnumerable<object> parts, SockChatClientMessage inst)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((int)inst);

            foreach (object part in parts)
            {
                sb.Append(Constants.SEPARATOR);
                sb.Append(part);
            }

            return sb.ToString();
        }

        public static void Send(this IWebSocketConnection conn, SockChatClientMessage inst, params object[] parts)
            => conn.Send(parts.Pack(inst));

        public static char AsChar(this bool b)
            => b ? '1' : '0';

        public static bool IsNumeric(this string str)
            => str.All(char.IsDigit);

        public static string RemoteAddress(this IWebSocketConnection conn)
            => (conn.ConnectionInfo.ClientIpAddress == @"127.0.0.1" || conn.ConnectionInfo.ClientIpAddress == @"::1")
                && conn.ConnectionInfo.Headers.ContainsKey(@"X-Real-IP")
                ? conn.ConnectionInfo.Headers[@"X-Real-IP"]
                : conn.ConnectionInfo.ClientIpAddress;

        public static string ToHexString(this byte[] arr)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in arr)
                sb.AppendFormat("{0:x2}", b);

            return sb.ToString();
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            lock (collection)
                foreach (T item in collection)
                    action(item);
        }

        public static string GetSignedHash(this string str, string key = null)
        {
            if (key == null)
                key = Utils.ReadFileOrDefault(@"login_key.txt", @"woomy");

            using (HMACSHA256 hash = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
                return hash.ComputeHash(Encoding.UTF8.GetBytes(str)).ToHexString();
        }

        public static void AppendNum(this StringBuilder sb, bool b)
            => sb.Append(b ? '1' : '0');

        public static string Serialise(this MessageFlags flags)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendNum(flags.HasFlag(MessageFlags.Bold));
            sb.AppendNum(flags.HasFlag(MessageFlags.Cursive));
            sb.AppendNum(flags.HasFlag(MessageFlags.Underline));
            sb.AppendNum(flags.HasFlag(MessageFlags.Colon));
            sb.AppendNum(flags.HasFlag(MessageFlags.Private));

            return sb.ToString();
        }
    }
}
