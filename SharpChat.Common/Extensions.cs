using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SharpChat {
    public static class Extensions {
        public static string GetIdString(this byte[] buffer) {
            const string id_chars = @"abcdefghijklmnopqrstuvwxyz0123456789-_ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder sb = new StringBuilder();
            foreach(byte b in buffer)
                sb.Append(id_chars[b % id_chars.Length]);
            return sb.ToString();
        }
    }
}
