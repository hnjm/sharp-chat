using Hamakaze;
using SharpChat.Configuration;
using SharpChat.Database;
using SharpChat.Database.Null;
using SharpChat.Database.SQLite;
using SharpChat.DataProvider;
using SharpChat.DataProvider.Null;
using SharpChat.WebSocket;
using SharpChat.WebSocket.Fleck;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SharpChat {
    public class Program {
        public const string CONFIG = @"sharpchat.cfg";
        public const ushort DEFAULT_PORT = 6770;

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

            string configFile = GetFlagArgument(args, @"--cfg") ?? CONFIG;

            if(!File.Exists(configFile) && configFile == CONFIG)
                ConvertConfiguration();

            using IConfig config = new StreamConfig(configFile);

            // Load database and data provider libraries
            LoadAssemblies(@"SharpChat.Database.*.dll");
            LoadAssemblies(@"SharpChat.DataProvider.*.dll");

            IDatabaseBackend databaseBackend;

            // Allow forcing a sqlite database through console flags
            string sqliteDbPath = GetFlagArgument(args, @"--dbpath");
            if(!string.IsNullOrEmpty(sqliteDbPath)) {
                Logger.Write($@"Forcing SQLite: {sqliteDbPath}");
                databaseBackend = new SQLiteDatabaseBackend(sqliteDbPath);
            } else {
                string databaseBackendName = GetFlagArgument(args, @"--dbb") ?? config.ReadValue(@"db");
                Type databaseBackendType = FindDatabaseBackendType(databaseBackendName);
                databaseBackend = (IDatabaseBackend)Activator.CreateInstance(databaseBackendType, config.ScopeTo($@"db:{databaseBackendName}"));
            }

            HttpClient.Instance.DefaultUserAgent = @"SharpChat/1.0";

            string dataProviderName = GetFlagArgument(args, @"--dpn") ?? config.ReadValue(@"dp");
            Type dataProviderType = FindDataProviderType(dataProviderName);
            IDataProvider dataProvider = (IDataProvider)Activator.CreateInstance(dataProviderType, config.ScopeTo($@"dp:{dataProviderName}"), HttpClient.Instance);

            string portArg = GetFlagArgument(args, @"--port") ?? config.ReadValue(@"chat:port");
            if(string.IsNullOrEmpty(portArg) || !ushort.TryParse(portArg, out ushort port))
                port = DEFAULT_PORT;

            using IWebSocketServer wss = new FleckWebSocketServer(new IPEndPoint(IPAddress.Any, port));
            using SockChatServer scs = new SockChatServer(config, wss, HttpClient.Instance, dataProvider, databaseBackend);

            using ManualResetEvent mre = new ManualResetEvent(false);
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; mre.Set(); };
            mre.WaitOne();

            if(dataProvider is IDisposable dpd)
                dpd.Dispose();
            databaseBackend.Dispose();
        }

        private static void LoadAssemblies(string pattern) {
            IEnumerable<string> files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), pattern);
            foreach(string file in files)
                Assembly.LoadFile(file);
        }

        private static Type FindTypeThroughAttribute<T>(Func<T, bool> compare)
            where T : Attribute {
            IEnumerable<Assembly> asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly asm in asms) {
                IEnumerable<Type> types = asm.GetExportedTypes();
                foreach(Type type in types) {
                    Attribute attr = type.GetCustomAttribute(typeof(T));
                    if(attr != null && compare((T)attr))
                        return type;
                }
            }
            return null;
        }

        private static Type FindDatabaseBackendType(string name) {
            return FindTypeThroughAttribute<DatabaseBackendAttribute>(a => a.Name == name) ?? typeof(NullDatabaseBackend);
        }

        private static Type FindDataProviderType(string name) {
            return FindTypeThroughAttribute<DataProviderAttribute>(a => a.Name == name) ?? typeof(NullDataProvider);
        }

        private static void ConvertConfiguration() {
            using Stream s = new FileStream(CONFIG, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            s.SetLength(0);
            s.Flush();

            using StreamWriter sw = new StreamWriter(s, new UTF8Encoding(false));

            const string sql_config = @"sqlite.txt";
            const string mdb_config = @"mariadb.txt";
            const string msz_config = @"login_key.txt";

            sw.WriteLine(@"# and ; can be used at the start of a line for comments.");
            sw.WriteLine();

            sw.WriteLine(@"# General Configuration");
            sw.WriteLine($@"#chat:port                 {DEFAULT_PORT}");
            sw.WriteLine($@"#chat:messages:maxLength   {ChatContext.DEFAULT_MSG_LENGTH_MAX}");
            sw.WriteLine($@"#chat:users:maxConnections {SockChatServer.DEFAULT_MAX_CONNECTIONS}");
            sw.WriteLine($@"#chat:flood:banDuration    {SockChatServer.DEFAULT_FLOOD_BAN_DURATION}");
            sw.WriteLine();

            sw.WriteLine(@"# Channels");
            sw.WriteLine(@"chat:channels lounge staff");
            sw.WriteLine();

            sw.WriteLine(@"# Lounge channel settings");
            sw.WriteLine(@"chat:channels:lounge:autoJoin true");
            sw.WriteLine();

            sw.WriteLine(@"# Staff channel settings");
            sw.WriteLine(@"chat:channels:staff:minRank 5");
            sw.WriteLine();

            sw.WriteLine(@"# Selected DataProvider (misuzu, null)");
            if(!File.Exists(msz_config))
                sw.WriteLine(@"dp null");
            else {
                sw.WriteLine(@"dp misuzu");
                sw.WriteLine();
                sw.WriteLine(@"# Misuzu DataProvider settings");
                sw.Write(@"dp:misuzu:secret   ");
                sw.Write(File.ReadAllText(msz_config).Trim());
                sw.WriteLine();
                sw.Write(@"dp:misuzu:endpoint ");
#if DEBUG
                sw.Write(@"https://misuzu.misaka.nl/_sockchat");
#else
                sw.Write(@"https://flashii.net/_sockchat");
#endif
                sw.WriteLine();
            }

            sw.WriteLine();

            bool hasMDB = File.Exists(mdb_config),
                 hasSQL = File.Exists(sql_config);

            sw.WriteLine(@"# Selected Database Backend (mariadb, sqlite, null)");
            if(hasMDB)
                sw.WriteLine(@"db mariadb");
            else if(hasSQL)
                sw.WriteLine(@"db sqlite");
            else
                sw.WriteLine(@"db null");
            sw.WriteLine();

            if(hasMDB) {
                string[] mdbCfg = File.ReadAllLines(mdb_config);
                sw.WriteLine(@"# MariaDB configuration");
                sw.WriteLine($@"db:mariadb:host {mdbCfg[0]}");
                if(mdbCfg.Length > 1)
                    sw.WriteLine($@"db:mariadb:user {mdbCfg[1]}");
                else
                    sw.WriteLine($@"#db:mariadb:user <username>");
                if(mdbCfg.Length > 2)
                    sw.WriteLine($@"db:mariadb:pass {mdbCfg[2]}");
                else
                    sw.WriteLine($@"#db:mariadb:pass <password>");
                if(mdbCfg.Length > 3)
                    sw.WriteLine($@"db:mariadb:db   {mdbCfg[3]}");
                else
                    sw.WriteLine($@"#db:mariadb:db   <database>");
                sw.WriteLine();
            }

            if(hasSQL) {
                string[] sqlCfg = File.ReadAllLines(sql_config);
                sw.WriteLine(@"# SQLite configuration");
                sw.WriteLine($@"db:sqlite:path {sqlCfg[0]}");
            }

            sw.Flush();
        }
    }
}
