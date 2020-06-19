using MySql.Data.MySqlClient;
using SharpChat.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SharpChat {
    public static class Database {
        private static string ConnectionString = null;

        public static bool HasDatabase
            => !string.IsNullOrWhiteSpace(ConnectionString);

        public static void ReadConfig() {
            string[] config = File.ReadAllLines(@"mariadb.txt");
            if (config.Length < 4)
                return;
            Init(config[0], config[1], config[2], config[3]);
        }

        public static void Init(string host, string username, string password, string database) {
            ConnectionString = new MySqlConnectionStringBuilder {
                Server = host,
                UserID = username,
                Password = password,
                Database = database,
                IgnorePrepare = false,
                OldGuids = false,
                TreatTinyAsBoolean = false,
                CharacterSet = @"utf8mb4",
                TreatBlobsAsUTF8 = false
            }.ToString();
        }

        public static void Deinit() {
            ConnectionString = null;
        }

        public static MySqlConnection GetConnection() {
            if (!HasDatabase)
                return null;

            MySqlConnection conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static int RunCommand(string command, params MySqlParameter[] parameters) {
            if (!HasDatabase)
                return 0;

            using MySqlConnection conn = GetConnection();
            using MySqlCommand cmd = conn.CreateCommand();
            if (parameters?.Length > 0)
                cmd.Parameters.AddRange(parameters);
            cmd.CommandText = command;
            cmd.CommandTimeout = 5;
            return cmd.ExecuteNonQuery();
        }

        public static MySqlDataReader RunQuery(string command, params MySqlParameter[] parameters) {
            if (!HasDatabase)
                return null;
            using MySqlConnection conn = GetConnection();
            using MySqlCommand cmd = conn.CreateCommand();
            if (parameters?.Length > 0)
                cmd.Parameters.AddRange(parameters);
            cmd.CommandText = command;
            cmd.CommandTimeout = 5;
            return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }

        public static object RunQueryOne(string command, params MySqlParameter[] parameters) {
            if (!HasDatabase)
                return null;
            using MySqlConnection conn = GetConnection();
            using MySqlCommand cmd = conn.CreateCommand();
            if (parameters?.Length > 0)
                cmd.Parameters.AddRange(parameters);
            cmd.CommandText = command;
            cmd.CommandTimeout = 5;
            cmd.Prepare();
            return cmd.ExecuteScalar();
        }

        private const long ID_EPOCH = 1588377600000;
        private static int IdCounter = 0;

        public static long GenerateId() {
            if (IdCounter > 200)
                IdCounter = 0;

            long id = 0;
            id |= (DateTimeOffset.Now.ToUnixTimeMilliseconds() - ID_EPOCH) << 8;
            id |= (ushort)(++IdCounter);
            return id;
        }

        public static void LogEvent(IChatEvent evt) {
            evt.SequenceId = GenerateId();

            List<MySqlParameter> extraParams = new List<MySqlParameter>();

            if (evt is IChatMessage msg)
                extraParams.Add(new MySqlParameter(@"text", msg.Text));
            if (evt is UserDisconnectEvent disconEvt)
                extraParams.Add(new MySqlParameter(@"leave", (int)disconEvt.Reason));

            StringBuilder sb = new StringBuilder();
            sb.Append(@"INSERT INTO `chat_events` (`event_id`, `event_sender`, `event_created`, `event_type`, `event_target`, `event_flags`");
            if (extraParams.Count > 0)
                sb.Append(@", `event_data`");
            sb.Append(@") VALUES (@id, @sender, FROM_UNIXTIME(@created), @type, @target, @flags");
            if (extraParams.Count > 0) {
                sb.Append(@", COLUMN_CREATE(");
                foreach (MySqlParameter param in extraParams)
                    sb.AppendFormat(@"'{0}', @{0}", param.ParameterName);
                sb.Append(@")");
            }
            sb.Append(@")");

            List<MySqlParameter> parameters = new List<MySqlParameter> {
                new MySqlParameter(@"id", evt.SequenceId),
                new MySqlParameter(@"sender", evt.Sender?.UserId < 1 ? null : (int?)evt.Sender.UserId),
                new MySqlParameter(@"created", evt.DateTime.ToUnixTimeSeconds()),
                new MySqlParameter(@"type", evt.GetType().FullName),
                new MySqlParameter(@"target", evt.Target.TargetName),
                new MySqlParameter(@"flags", (byte)evt.Flags)
            };
            parameters.AddRange(extraParams);

            RunCommand(sb.ToString(), parameters.ToArray());
        }

        public static void DeleteEvent(IChatEvent evt) {
            RunCommand(
                @"UPDATE IGNORE `chat_events` SET `event_deleted` = NOW() WHERE `event_id` = @id AND `event_deleted` IS NULL",
                new MySqlParameter(@"id", evt.SequenceId)
            );
        }
    }
}
