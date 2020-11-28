using SharpChat;
using System;
using System.IO;
using static System.Console;

namespace SharpChatTest {
    public static class Program {
        public static void Main() {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            WriteLine(@"   _____ __                     ________          __ ______          __ ");
            WriteLine(@"  / ___// /_  ____ __________  / ____/ /_  ____ _/ //_  __/__  _____/ /_");
            WriteLine(@"  \__ \/ __ \/ __ `/ ___/ __ \/ /   / __ \/ __ `/ __// / / _ \/ ___/ __/");
            WriteLine(@" ___/ / / / / /_/ / /  / /_/ / /___/ / / / /_/ / /_ / / /  __(__  ) /_  ");
            WriteLine(@"/____/_/ /_/\__,_/_/  / .___/\____/_/ /_/\__,_/\__//_/  \___/____/\__/  ");
            WriteLine(@"                     / _/       Sock Chat Protocol Implementation Tester");

            ushort port = (ushort)RNG.Next(5001, 49151);
            string dbPath = @"sct-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + @".db";

            WriteLine(@"Starting SharpChat server...");
            using SharpChatExec sc = new SharpChatExec(port, dbPath);

            ReadLine();
        }
    }
}
