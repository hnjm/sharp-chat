using System;
using System.Security.Cryptography;
using System.Text;

namespace SharpChat {
    public static class RNG {
        private static object Lock { get; } = new object();
        private static Random NormalRandom { get; } = new Random();
        private static RandomNumberGenerator SecureRandom { get; } = RandomNumberGenerator.Create();

        public static int Next() {
            lock (Lock)
                return NormalRandom.Next();
        }

        public static int Next(int max) {
            lock (Lock)
                return NormalRandom.Next(max);
        }

        public static int Next(int min, int max) {
            lock (Lock)
                return NormalRandom.Next(min, max);
        }

        public static void NextBytes(byte[] buffer) {
            lock(Lock)
                SecureRandom.GetBytes(buffer);
        }

        public const string ID_CHARS = @"abcdefghijklmnopqrstuvwxyz0123456789-_ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string NextIdString(int length, string chars = ID_CHARS) {
            StringBuilder sb = new StringBuilder();
            byte[] buffer = new byte[length];
            foreach(byte b in buffer)
                sb.Append(chars[b % chars.Length]);
            return sb.ToString();
        }
    }
}
