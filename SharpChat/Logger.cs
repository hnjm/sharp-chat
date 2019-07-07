using System;

namespace SharpChat
{
    public static class Logger
    {
        public static void Write(string str)
        {
            Console.WriteLine(string.Format(@"[{1}] {0}", str, DateTime.Now));
        }
    }
}
