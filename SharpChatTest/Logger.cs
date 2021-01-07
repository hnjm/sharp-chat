using System;

namespace SharpChatTest {
    public static class Logger {
        private static readonly object LogLock = new object();

        public static void Write(string text, ConsoleColor color = ConsoleColor.Gray) {
            lock(LogLock) {
                Console.ForegroundColor = color;
                Console.Write(text);
            }
        }

        public static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray) {
            lock(LogLock) {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
            }
        }

        public static void ClientWrite(string text)
            => Write(text, ConsoleColor.Cyan);
        public static void ClientWriteLine(string text)
            => WriteLine(text, ConsoleColor.Cyan);

        public static void ServerWrite(string text)
            => Write(text, ConsoleColor.Magenta);
        public static void ServerWriteLine(string text)
            => WriteLine(text, ConsoleColor.Magenta);

        public static void ErrorWrite(string text)
            => Write(text, ConsoleColor.Red);
        public static void ErrorWriteLine(string text)
            => WriteLine(text, ConsoleColor.Red);
    }
}
