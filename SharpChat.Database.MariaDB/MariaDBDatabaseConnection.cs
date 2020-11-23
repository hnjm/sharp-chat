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
            command.CommandTimeout = 5;
            return new MariaDBDatabaseCommand(this, command);
        }

        private bool IsDisposed;
        ~MariaDBDatabaseConnection()
            => Dispose(false);
        public void Dispose()
            => Dispose(true);
        private void Dispose(bool disposing) {
            if(IsDisposed)
                return;
            IsDisposed = true;

            Connection.Dispose();

            if(disposing)
                GC.SuppressFinalize(this);
        }
    }
}
