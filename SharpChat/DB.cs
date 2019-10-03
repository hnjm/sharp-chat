using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace SharpChat {
    public static class DB {
        public static MySqlConnection Connection { get; private set; }

        public static void Connect(
            string server,
            string username,
            string password,
            string database
        ) {
            bool unixSock = false;
            if(server[0..5] == @"unix:") {
                unixSock = true;
                server = server[4..];
            }

            Connection = new MySqlConnection(new MySqlConnectionStringBuilder {
                Server = server,
                UserID = username,
                Port = 3306,
                Password = password,
                Database = database,
                CharacterSet = @"utf8mb4",
                ConnectionProtocol = unixSock ? MySqlConnectionProtocol.Unix : MySqlConnectionProtocol.Socket,
                IgnorePrepare = false,
            }.ToString());

            try {
                Connection.Open();
            } catch(MySqlException ex) {
                Logger.Write(ex.Message);
                Connection = null;
            }
        }

        private static MySqlCommand LogEventCommand { get; set; }

        public static void LogEvent(IChatMessage msg) {
            if (Connection == null)
                return;

            lock (Connection) {
                if (LogEventCommand == null) {
                    LogEventCommand = Connection.CreateCommand();
                    LogEventCommand.CommandText = @"INSERT INTO `sharp_log` (`user_id`, `user_name`, `user_colour`, `log_target`, `log_datetime`, `log_text`, `log_flags`) VALUES (@uid, @uname, @ucol, @ltarg, @ldt, @ltext, @lflg)";
                }

                LogEventCommand.Parameters.Clear();
                LogEventCommand.Parameters.AddWithValue(@"uid", msg.Sender.UserId);
                LogEventCommand.Parameters.AddWithValue(@"uname", msg.Sender.Username);
                LogEventCommand.Parameters.AddWithValue(@"ucol", msg.Sender.Colour.Raw);
                LogEventCommand.Parameters.AddWithValue(@"ltarg", msg.Target.TargetName);
                LogEventCommand.Parameters.AddWithValue(@"ldt", msg.DateTime.UtcDateTime.ToString(@"yyyy-MM-dd H:mm:ss"));
                LogEventCommand.Parameters.AddWithValue(@"ltext", msg.Text);
                LogEventCommand.Parameters.AddWithValue(@"lflg", (int)msg.Flags);
                LogEventCommand.Prepare();
                LogEventCommand.ExecuteNonQuery();
            }

        }
    }
}
