using System;

namespace SharpChat {
    public static class RNG {
        private static readonly Random random = new Random();
        private static readonly object randomLock = new object();

        public static int Next() {
            lock (randomLock)
                return random.Next();
        }

        public static int Next(int max) {
            lock (randomLock)
                return random.Next(max);
        }

        public static int Next(int min, int max) {
            lock (randomLock)
                return random.Next(min, max);
        }
    }
}
