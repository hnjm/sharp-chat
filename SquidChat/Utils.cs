using System;
using System.Collections.Generic;
using System.Text;

namespace SquidChat
{
    public static class Utils
    {
        public static string UnixNow
            => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
    }
}
