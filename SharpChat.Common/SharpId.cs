using System;
using System.Threading;

namespace SharpChat {
    public static class SharpId {
        private const long EPOCH = 1588377600000;
        private static int Counter = 0;

        public static long Next()
            => ((DateTimeOffset.Now.ToUnixTimeMilliseconds() - EPOCH) << 8)
                | (ushort)(Interlocked.Increment(ref Counter) & 0xFFFF);
    }
}
