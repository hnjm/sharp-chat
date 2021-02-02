using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SharpChat {
    public static class Logger {
        public static void Write()
            => Console.WriteLine();

        [Conditional(@"DEBUG")]
        public static void Debug()
            => Write();

        public static void Write(string str)
            => Console.WriteLine(string.Format(@"[{1}] {0}", str, DateTime.Now));

        [Conditional(@"DEBUG")]
        public static void Debug(string str)
            => Write(str);

        public static void Write(byte[] bytes)
            => Write(Encoding.UTF8.GetString(bytes));

        [Conditional(@"DEBUG")]
        public static void Debug(byte[] bytes)
            => Write(bytes);

        public static void Write(object obj)
            => Write(obj?.ToString() ?? string.Empty);

        [Conditional(@"DEBUG")]
        public static void Debug(object obj)
            => Write(obj);

        public static void Write(IEnumerable<object> objs)
            => Write(string.Join(@", ", objs));

        [Conditional(@"DEBUG")]
        public static void Debug(IEnumerable<object> objs)
            => Write(objs);
    }
}
