using MySql.Data.MySqlClient;
using SharpChat.Events;
using System.Data;

namespace SharpChat {
    public static class DB {
        public static MySqlConnection Connection { get; private set; }

        private static string Server;
        private static string Username;
        private static string Password;
        private static string Database;
        private static bool IsUnixSocket;

        public static void Connect(
            string server,
            string username,
            string password,
            string database
        ) {
            IsUnixSocket = server[0..5] == @"unix:";
            Server = IsUnixSocket ? server[5..] : server;
            Username = username;
            Password = password;
            Database = database;
            Connect();
        }

        private static bool Connect() {
            Connection = new MySqlConnection(new MySqlConnectionStringBuilder {
                Server = Server,
                UserID = Username,
                Port = 3306,
                Password = Password,
                Database = Database,
                CharacterSet = @"utf8mb4",
                ConnectionProtocol = IsUnixSocket ? MySqlConnectionProtocol.Unix : MySqlConnectionProtocol.Socket,
                IgnorePrepare = false,
            }.ToString());

            try {
                Connection.Open();
            } catch (MySqlException ex) {
                Logger.Write(ex.Message);
                Connection = null;
                return false;
            }

            return true;
        }

        private static bool EnsureConnected() {
            if (Connection == null)
                return false;

            if (Connection.State != ConnectionState.Open && !Connect()) {
                Connection = null;
                return false;
            }

            return true;
        }

        private static MySqlCommand LogEventCommand { get; set; }
        private static readonly object LogEventLock = new object();

        public static void LogEvent(IChatEvent evt) {
            if (!EnsureConnected())
                return;

            lock (LogEventLock) {
                if (LogEventCommand == null) {
                    LogEventCommand = Connection.CreateCommand();
                    LogEventCommand.CommandText = @"INSERT INTO `sharp_log` (`user_id`, `user_name`, `user_colour`, `log_target`, `log_datetime`, `log_text`, `log_flags`) VALUES (@uid, @uname, @ucol, @ltarg, @ldt, @ltext, @lflg)";
                }

                string ltext = string.Empty;

                switch (evt) {
                    case IChatMessage msg:
                        ltext = msg.Text;
                        break;
                    case UserConnectEvent _:
                        ltext = @"connect";
                        break;
                    case UserDisconnectEvent ude:
                        ltext = $@"disconnect:{ude.Reason}";
                        break;
                    case UserChannelJoinEvent _:
                        ltext = @"join";
                        break;
                    case UserChannelLeaveEvent _:
                        ltext = @"leave";
                        break;
                }

                if (string.IsNullOrEmpty(ltext))
                    return;

                LogEventCommand.Parameters.Clear();
                LogEventCommand.Parameters.AddWithValue(@"uid", evt.Sender.UserId);
                LogEventCommand.Parameters.AddWithValue(@"uname", evt.Sender.Username);
                LogEventCommand.Parameters.AddWithValue(@"ucol", evt.Sender.Colour.Raw);
                LogEventCommand.Parameters.AddWithValue(@"ltarg", evt.Target.TargetName);
                LogEventCommand.Parameters.AddWithValue(@"ldt", evt.DateTime.UtcDateTime.ToString(@"yyyy-MM-dd H:mm:ss"));
                LogEventCommand.Parameters.AddWithValue(@"lflg", (int)evt.Flags);
                LogEventCommand.Parameters.AddWithValue(@"ltext", ltext);
                LogEventCommand.Prepare();
                LogEventCommand.ExecuteNonQuery();
            }
        }
    }
}
