using SharpChat;
using SharpChatTest.SockChat;
using System;
using System.IO;
using System.Threading;

namespace SharpChatTest {
    public static class Program {
        public static void Main() {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Logger.WriteLine(@"   _____ __                     ________          __ ______          __ ");
            Logger.WriteLine(@"  / ___// /_  ____ __________  / ____/ /_  ____ _/ //_  __/__  _____/ /_");
            Logger.WriteLine(@"  \__ \/ __ \/ __ `/ ___/ __ \/ /   / __ \/ __ `/ __// / / _ \/ ___/ __/");
            Logger.WriteLine(@" ___/ / / / / /_/ / /  / /_/ / /___/ / / / /_/ / /_ / / /  __(__  ) /_  ");
            Logger.WriteLine(@"/____/_/ /_/\__,_/_/  / .___/\____/_/ /_/\__,_/\__//_/  \___/____/\__/  ");
            Logger.WriteLine(@"                     / _/       Sock Chat Protocol Implementation Tester");

            ushort port = (ushort)RNG.Next(10000, 40000);
            string dbPath = @"sct-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + @".db";

            Logger.WriteLine(@"Starting SharpChat server...");
            using SharpChatExec sc = new SharpChatExec(port, dbPath);

            Thread.Sleep(3000);

            using SockChatClient client = new SockChatClient(port, 1);

            Thread.Sleep(2000);

            client.SendLogin();

            Console.ReadLine();
        }
    }
}
