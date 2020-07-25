using SharpChat.Flashii;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SharpChat {
    public class Program {
        public const ushort PORT = 6770;

        public static void Main(string[] args) {
            Console.WriteLine(@"   _____ __                     ________          __ ");
            Console.WriteLine(@"  / ___// /_  ____ __________  / ____/ /_  ____ _/ /_");
            Console.WriteLine(@"  \__ \/ __ \/ __ `/ ___/ __ \/ /   / __ \/ __ `/ __/");
            Console.WriteLine(@" ___/ / / / / /_/ / /  / /_/ / /___/ / / / /_/ / /_  ");
            Console.WriteLine(@"/____/_/ /_/\__,_/_/  / .___/\____/_/ /_/\__,_/\__/  ");
            Console.WriteLine(@"                     / _/            Sock Chat Server");
#if DEBUG
            Console.WriteLine(@"============================================ DEBUG ==");
#endif

#if DEBUG
            Console.WriteLine(@"HOLD A KEY TO START A TEST NOW");
            Thread.Sleep(1000);
            if (Console.KeyAvailable)
                switch (Console.ReadKey(true).Key) {
                    case ConsoleKey.F:
                        TestMisuzuAuth();
                        return;
                }
#endif

            Database.ReadConfig();

            using ManualResetEvent mre = new ManualResetEvent(false);
            using SockChatServer scs = new SockChatServer(PORT);
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; mre.Set(); };
            mre.WaitOne();
        }

#if DEBUG
        private static void TestMisuzuAuth() {
            Console.WriteLine($@"Enter token found on {FlashiiUrls.BASE_URL}/login:");
            string[] token = Console.ReadLine().Split(new[] { '_' }, 2);

            System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"SharpChat");

            FlashiiAuth authRes = FlashiiAuth.Attempt(httpClient, new FlashiiAuthRequest {
                UserId = int.Parse(token[0]), Token = token[1], IPAddress = @"1.2.4.8"
            }).GetAwaiter().GetResult();

            if(authRes.Success) {
                Console.WriteLine(@"Auth success!");
                Console.WriteLine($@" User ID:   {authRes.UserId}");
                Console.WriteLine($@" Username:  {authRes.Username}");
                Console.WriteLine($@" Colour:    {authRes.ColourRaw:X8}");
                Console.WriteLine($@" Hierarchy: {authRes.Rank}");
                Console.WriteLine($@" Silenced:  {authRes.SilencedUntil}");
                Console.WriteLine($@" Perms:     {authRes.Permissions}");
            } else {
                Console.WriteLine($@"Auth failed: {authRes.Reason}");
                return;
            }

            Console.WriteLine(@"Bumping last seen...");
            FlashiiBump.Submit(httpClient, new[] { new ChatUser(authRes) });

            Console.WriteLine(@"Fetching ban list...");
            IEnumerable<FlashiiBan> bans = FlashiiBan.GetList(httpClient).GetAwaiter().GetResult();
            Console.WriteLine($@"{bans.Count()} BANS");
            foreach(FlashiiBan ban in bans) {
                Console.WriteLine($@"BAN INFO");
                Console.WriteLine($@" User ID:    {ban.UserId}");
                Console.WriteLine($@" Username:   {ban.Username}");
                Console.WriteLine($@" IP Address: {ban.UserIP}");
                Console.WriteLine($@" Expires:    {ban.Expires}");
            }
        }
#endif
    }
}
