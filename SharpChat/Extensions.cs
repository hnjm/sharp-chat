using Fleck;
using System;
using System.Collections.Generic;
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
    }
}
