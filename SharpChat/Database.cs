using MySql.Data.MySqlClient;
using SharpChat.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SharpChat {
    public static partial class Database {
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
            RunMigrations();
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

            try {
                using MySqlConnection conn = GetConnection();
                using MySqlCommand cmd = conn.CreateCommand();
                if (parameters?.Length > 0)
                    cmd.Parameters.AddRange(parameters);
                cmd.CommandText = command;
                cmd.CommandTimeout = 5;
                return cmd.ExecuteNonQuery();
            } catch (MySqlException) { }

            return 0;
        }

        public static MySqlDataReader RunQuery(string command, params MySqlParameter[] parameters) {
            if (!HasDatabase)
                return null;

            try {
                MySqlConnection conn = GetConnection();
                MySqlCommand cmd = conn.CreateCommand();
                if (parameters?.Length > 0)
                    cmd.Parameters.AddRange(parameters);
                cmd.CommandText = command;
                cmd.CommandTimeout = 5;
                return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            } catch (MySqlException) { }

            return null;
        }

        public static object RunQueryValue(string command, params MySqlParameter[] parameters) {
            if (!HasDatabase)
                return null;

            try {
                using MySqlConnection conn = GetConnection();
                using MySqlCommand cmd = conn.CreateCommand();
                if (parameters?.Length > 0)
                    cmd.Parameters.AddRange(parameters);
                cmd.CommandText = command;
                cmd.CommandTimeout = 5;
                cmd.Prepare();
                return cmd.ExecuteScalar();
            } catch (MySqlException) { }

            return null;
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
            if(evt.SequenceId < 1)
                evt.SequenceId = GenerateId();

            RunCommand(
                @"INSERT INTO `sqc_events` (`event_id`, `event_created`, `event_type`, `event_target`, `event_flags`, `event_data`"
                + @", `event_sender`, `event_sender_name`, `event_sender_colour`, `event_sender_rank`, `event_sender_nick`, `event_sender_perms`)"
                + @" VALUES (@id, FROM_UNIXTIME(@created), @type, @target, @flags, @data"
                + @", @sender, @sender_name, @sender_colour, @sender_rank, @sender_nick, @sender_perms)",
                new MySqlParameter(@"id", evt.SequenceId),
                new MySqlParameter(@"created", evt.DateTime.ToUnixTimeSeconds()),
                new MySqlParameter(@"type", evt.GetType().FullName),
                new MySqlParameter(@"target", evt.Target.TargetName),
                new MySqlParameter(@"flags", (byte)evt.Flags),
                new MySqlParameter(@"data", JsonSerializer.SerializeToUtf8Bytes(evt, evt.GetType())),
                new MySqlParameter(@"sender", evt.Sender?.UserId < 1 ? null : (long?)evt.Sender.UserId),
                new MySqlParameter(@"sender_name", evt.Sender?.Username),
                new MySqlParameter(@"sender_colour", evt.Sender?.Colour.Raw),
                new MySqlParameter(@"sender_rank", evt.Sender?.Hierarchy),
                new MySqlParameter(@"sender_nick", evt.Sender?.Nickname),
                new MySqlParameter(@"sender_perms", evt.Sender?.Permissions)
            );
        }

        public static void DeleteEvent(IChatEvent evt) {
            RunCommand(
                @"UPDATE IGNORE `sqc_events` SET `event_deleted` = NOW() WHERE `event_id` = @id AND `event_deleted` IS NULL",
                new MySqlParameter(@"id", evt.SequenceId)
            );
        }

        private static IChatEvent ReadEvent(MySqlDataReader reader, IPacketTarget target = null) {
            Type evtType = Type.GetType(reader.GetString(@"event_type"));
            IChatEvent evt = JsonSerializer.Deserialize(reader.GetString(@"event_data"), evtType) as IChatEvent;
            evt.SequenceId = reader.GetInt64(@"event_id");
            evt.Target = target;
            evt.TargetName = target?.TargetName ?? reader.GetString(@"event_target");
            evt.Flags = (ChatMessageFlags)reader.GetByte(@"event_flags");
            evt.DateTime = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(@"event_created"));

            if (!reader.IsDBNull(reader.GetOrdinal(@"event_sender"))) {
                evt.Sender = new BasicUser {
                    UserId = reader.GetInt64(@"event_sender"),
                    Username = reader.GetString(@"event_sender_name"),
                    Colour = new ChatColour(reader.GetInt32(@"event_sender_colour")),
                    Hierarchy = reader.GetInt32(@"event_sender_rank"),
                    Nickname = reader.IsDBNull(reader.GetOrdinal(@"event_sender_nick")) ? null : reader.GetString(@"event_sender_nick"),
                    Permissions = (ChatUserPermissions)reader.GetInt32(@"event_sender_perms")
                };
            }

            return evt;
        }

        public static IEnumerable<IChatEvent> GetEvents(IPacketTarget target, int amount, int offset) {
            List<IChatEvent> events = new List<IChatEvent>();

            try {
                using MySqlDataReader reader = RunQuery(
                    @"SELECT `event_id`, `event_type`, `event_flags`, `event_data`"
                    + @", `event_sender`, `event_sender_name`, `event_sender_colour`, `event_sender_rank`, `event_sender_nick`, `event_sender_perms`"
                    + @", UNIX_TIMESTAMP(`event_created`) AS `event_created`"
                    + @" FROM `sqc_events`"
                    + @" WHERE `event_deleted` IS NULL AND `event_target` = @target"
                    + @" ORDER BY `event_created` DESC"
                    + @" LIMIT @amount OFFSET @offset",
                    new MySqlParameter(@"target", target.TargetName),
                    new MySqlParameter(@"amount", amount),
                    new MySqlParameter(@"offset", offset)
                );

                while (reader.Read()) {
                    IChatEvent evt = ReadEvent(reader, target);
                    if (evt != null)
                        events.Add(evt);
                }
            } catch (MySqlException) {}

            return events;
        }

        public static IChatEvent GetEvent(long seqId) {
            try {
                using MySqlDataReader reader = RunQuery(
                    @"SELECT `event_id`, `event_type`, `event_flags`, `event_data`"
                    + @", `event_sender`, `event_sender_name`, `event_sender_colour`, `event_sender_rank`, `event_sender_nick`, `event_sender_perms`"
                    + @", UNIX_TIMESTAMP(`event_created`) AS `event_created`"
                    + @" FROM `sqc_events`"
                    + @" WHERE `event_id` = @id",
                    new MySqlParameter(@"id", seqId)
                );

                while (reader.Read()) {
                    IChatEvent evt = ReadEvent(reader);
                    if (evt != null)
                        return evt;
                }
            } catch(MySqlException) { }

            return null;
        }
    }
}
