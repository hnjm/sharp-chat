using MySql.Data.MySqlClient;
using System;

namespace SharpChat.Database.MariaDB {
    public class MariaDBDatabaseConnection : IDatabaseConnection {
        private MySqlConnection Connection { get; }

        public MariaDBDatabaseConnection(string dsn) {
            Connection = new MySqlConnection(dsn ?? throw new ArgumentNullException(nameof(dsn)));
            Connection.Open();
        }

        public IDatabaseCommand CreateCommand(object query) {
            MySqlCommand command = Connection.CreateCommand();
            command.CommandText = query.ToString();
            return new MariaDBDatabaseCommand(this, command);
        }

        private bool IsDisposed;

        ~MariaDBDatabaseConnection()
            => DoDispose();

        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }

        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;
            Connection.Dispose();
        }
    }
}
