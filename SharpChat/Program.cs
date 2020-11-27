using SharpChat.Bans;
using SharpChat.Database;
using SharpChat.Database.MariaDB;
using SharpChat.Database.Null;
using SharpChat.Database.SQLite;
using SharpChat.DataProvider.Misuzu;
using SharpChat.Users;
using SharpChat.Users.Auth;
using SharpChat.WebSocket;
using SharpChat.WebSocket.Fleck;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace SharpChat {
    public class Program {
        public const string SQL_CONFIG = @"sqlite.txt";
        public const string MDB_CONFIG = @"mariadb.txt";
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

            Console.Write(@"Press a key to start a test");
            for(int i = 10; i > 0; --i) {
                Thread.Sleep(100);
                Console.Write('.');
                if(Console.KeyAvailable) {
                    Console.WriteLine();
                    switch(Console.ReadKey(true).Key) {
                        case ConsoleKey.F:
                            TestMisuzuAuth();
                            return;
                        default:
                            break;
                    }
                }
            }
            Console.WriteLine();
#endif

            using ManualResetEvent mre = new ManualResetEvent(false);

            IDatabaseBackend db = new NullDatabaseBackend();

            // TODO: Make this not suck
            if(!File.Exists(MDB_CONFIG)) {
                Console.WriteLine(@"MariaDB configuration is missing. Attempting SQLite...");
                if(!File.Exists(SQL_CONFIG)) {
                    Console.WriteLine(@"SQLite configuration is also missing. Skipping database connection...");
                } else {
                    string[] config = File.ReadAllLines(SQL_CONFIG);
                    if(config.Length < 1) {
                        Console.WriteLine(@"SQLite configuration does not contain sufficient information. Skipping database connection...");
                    } else {
                        db.Dispose();
                        db = new SQLiteDatabaseBackend(config[0]);
                    }
                }
            } else {
                string[] config = File.ReadAllLines(MDB_CONFIG);
                if(config.Length < 4) {
                    Console.WriteLine(@"MariaDB configuration does not contain sufficient information. Skipping database connection...");
                } else {
                    db.Dispose();
                    db = new MariaDBDatabaseBackend(config[0], config[1], config[2], config[3]);
                }
            }

            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"SharpChat");

            using IWebSocketServer wss = new FleckWebSocketServer(PORT);
            using SockChatServer scs = new SockChatServer(wss, httpClient, new MisuzuDataProvider(httpClient), db);

            Console.CancelKeyPress += (s, e) => { e.Cancel = true; mre.Set(); };
            mre.WaitOne();

            db.Dispose();
        }

#if DEBUG
        private static void TestMisuzuAuth() {
            Console.WriteLine($@"Enter token found on {MisuzuUrls.BASE_URL}/login:");
            string[] token = Console.ReadLine().Split(new[] { '_' }, 2);

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"SharpChat");

            IDataProvider dataProvider = new MisuzuDataProvider(httpClient);

            long userId = long.Parse(token[0]);
            IPAddress remoteAddr = IPAddress.Parse(@"1.2.4.8");

            for(int i = 0; i < 100; ++i) {
                IUserAuthResponse authRes;
                try {
                    authRes = dataProvider.UserAuthClient.AttemptAuth(new UserAuthRequest(userId, token[1], remoteAddr));

                    Console.WriteLine(@"Auth success!");
                    Console.WriteLine($@" User ID:   {authRes.UserId}");
                    Console.WriteLine($@" Username:  {authRes.Username}");
                    Console.WriteLine($@" Colour:    {authRes.Colour.Raw:X8}");
                    Console.WriteLine($@" Hierarchy: {authRes.Rank}");
                    Console.WriteLine($@" Silenced:  {authRes.SilencedUntil}");
                    Console.WriteLine($@" Perms:     {authRes.Permissions}");
                } catch(UserAuthFailedException ex) {
                    Console.WriteLine($@"Auth failed: {ex.Message}");
                    return;
                }

                Console.WriteLine(@"Bumping last seen...");
                dataProvider.UserBumpClient.SubmitBumpUsers(new[] { new ChatUser(authRes) });

                Console.WriteLine(@"Fetching ban list...");
                IEnumerable<IBanRecord> bans = dataProvider.BanClient.GetBanList();
                Console.WriteLine($@"{bans.Count()} BANS");
                foreach(IBanRecord ban in bans) {
                    Console.WriteLine($@"BAN INFO");
                    Console.WriteLine($@" User ID:    {ban.UserId}");
                    Console.WriteLine($@" Username:   {ban.Username}");
                    Console.WriteLine($@" IP Address: {ban.UserIP}");
                    Console.WriteLine($@" Expires:    {ban.Expires}");
                }
            }
        }
#endif
    }
}
