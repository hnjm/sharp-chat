using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SharpChat {
    public static class Extensions {
        public static string GetSignedHash(this string str, string key = null)
            => Encoding.UTF8.GetBytes(str).GetSignedHash(key);

        public static string GetSignedHash(this byte[] bytes, string key = null) {
            if (key == null)
                key = File.Exists(@"login_key.txt") ? File.ReadAllText(@"login_key.txt") : @"woomy";

            StringBuilder sb = new StringBuilder();

            using (HMACSHA256 algo = new HMACSHA256(Encoding.UTF8.GetBytes(key))) {
                byte[] hash = algo.ComputeHash(bytes);

                foreach (byte b in hash)
                    sb.AppendFormat(@"{0:x2}", b);
            }

            return sb.ToString();
        }

        public static string GetIdString(this byte[] buffer) {
            const string id_chars = @"abcdefghijklmnopqrstuvwxyz0123456789-_ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder sb = new StringBuilder();
            foreach(byte b in buffer)
                sb.Append(id_chars[b % id_chars.Length]);
            return sb.ToString();
        }
    }
}
