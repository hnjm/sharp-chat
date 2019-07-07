using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat
{
    public static class Utils
    {
        public static string UnixNow
            => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
    }
}
