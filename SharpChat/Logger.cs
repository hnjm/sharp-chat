using System;
using System.Diagnostics;
using System.Text;

namespace SharpChat {
    public static class Logger {
        public static void Write(string str)
            => Console.WriteLine(string.Format(@"[{1}] {0}", str, DateTime.Now));

        public static void Write(byte[] bytes)
            => Write(Encoding.UTF8.GetString(bytes));

        public static void Write(object obj)
            => Write(obj?.ToString() ?? string.Empty);

        [Conditional(@"DEBUG")]
        public static void Debug(string str)
            => Write(str);

        [Conditional(@"DEBUG")]
        public static void Debug(byte[] bytes)
            => Write(bytes);

        [Conditional(@"DEBUG")]
        public static void Debug(object obj)
            => Write(obj);
    }
}
