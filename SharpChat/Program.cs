using SharpChat.Database;
using SharpChat.Database.MariaDB;
using SharpChat.Database.Null;
using SharpChat.Database.SQLite;
using SharpChat.DataProvider;
using SharpChat.DataProvider.Misuzu;
using SharpChat.WebSocket;
using SharpChat.WebSocket.Fleck;
using System;
using System.IO;
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

            IDataProvider dataProvider = new MisuzuDataProvider(httpClient);

            using IWebSocketServer wss = new FleckWebSocketServer(PORT);
            using SockChatServer scs = new SockChatServer(wss, httpClient, dataProvider, db);

            Console.CancelKeyPress += (s, e) => { e.Cancel = true; mre.Set(); };
            mre.WaitOne();

            db.Dispose();
        }
    }
}
