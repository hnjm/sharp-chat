using SharpChat.Database;
using SharpChat.Database.MariaDB;
using SharpChat.Database.Null;
using SharpChat.Database.SQLite;
using SharpChat.DataProvider;
using SharpChat.DataProvider.Misuzu;
using SharpChat.DataProvider.Null;
using SharpChat.WebSocket;
using SharpChat.WebSocket.Fleck;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace SharpChat {
    public class Program {
        public const string SQL_CONFIG = @"sqlite.txt";
        public const string MDB_CONFIG = @"mariadb.txt";
        public const ushort PORT = 6770;

        private static string GetFlagArgument(string[] args, string flag) {
            int offset = Array.IndexOf(args, flag) + 1;
            return offset < 1 ? null : args.ElementAtOrDefault(offset);
        }

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

            string databaseBackend = GetFlagArgument(args, @"--dbb");
            string dataProviderName = GetFlagArgument(args, @"--dpn");

            using ManualResetEvent mre = new ManualResetEvent(false);

            // TODO: This still sucks
            IDatabaseBackend db;
            switch(databaseBackend) {
                case @"mariadb":
                    if(!File.Exists(MDB_CONFIG)) {
                        Console.WriteLine(@"MariaDB configuration is missing.");
                        return;
                    }

                    string[] mdbCfg = File.ReadAllLines(MDB_CONFIG);
                    db = new MariaDBDatabaseBackend(mdbCfg[0], mdbCfg[1], mdbCfg[2], mdbCfg[3]);
                    Console.WriteLine(@"MariaDB database backend created!");
                    break;

                case @"sqlite":
                    string dbPath = GetFlagArgument(args, @"--dbpath");

                    if(string.IsNullOrEmpty(dbPath))
                        if(!File.Exists(SQL_CONFIG)) {
                            Console.WriteLine(@"SQLite configuration is missing.");
                            return;
                        } else
                            dbPath = File.ReadAllLines(SQL_CONFIG)[0];
                    else
                        Console.WriteLine(@"Using database path provided in arguments.");

                    db = new SQLiteDatabaseBackend(dbPath);
                    Console.WriteLine(@"SQLite database backend created!");
                    break;

                case @"null":
                    db = new NullDatabaseBackend();
                    Console.WriteLine(@"Null database backend created!");
                    break;

                default:
                    Console.WriteLine(@"No database flag provided. Checking based on existence of configs...");

                    if(File.Exists(MDB_CONFIG)) {
                        Console.WriteLine(@"MariaDB configuration found.");
                        goto case @"mariadb";
                    }

                    if(File.Exists(SQL_CONFIG)) {
                        Console.WriteLine(@"SQLite configuration found.");
                        goto case @"sqlite";
                    }

                    Console.WriteLine(@"No configurations found, continuing without a database...");
                    goto case @"null";
            }

            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"SharpChat");

            // TODO: see database backend selection process
            IDataProvider dataProvider;
            switch(dataProviderName) {
                case @"misuzu":
                    dataProvider = new MisuzuDataProvider(httpClient);
                    Console.WriteLine(@"Misuzu data provider created!");
                    break;

                case @"null":
                    dataProvider = new NullDataProvider();
                    Console.WriteLine(@"Null data provider created!");
                    break;

                default:
                    Console.WriteLine(@"No data provider flag provided. Checking based on existence of configs...");

                    if(File.Exists(MisuzuConstants.LOGIN_KEY)) {
                        Console.WriteLine(@"Misuzu configuration found.");
                        goto case @"misuzu";
                    }

                    Console.WriteLine(@"No configurations found, continuing without a data provider...");
                    goto case @"null";
            }

            using IWebSocketServer wss = new FleckWebSocketServer(PORT);
            using SockChatServer scs = new SockChatServer(wss, httpClient, dataProvider, db);

            Console.CancelKeyPress += (s, e) => { e.Cancel = true; mre.Set(); };
            mre.WaitOne();

            db.Dispose();
        }
    }
}
