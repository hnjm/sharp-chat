using System;
using System.Data.SQLite;

namespace SharpChat.Database.SQLite {
    public class SQLiteDatabaseConnection : IDatabaseConnection {
        private SQLiteConnection Connection { get; }

        public SQLiteDatabaseConnection(string dsn) {
            Connection = new SQLiteConnection(dsn ?? throw new ArgumentNullException(nameof(dsn)));
            Connection.Open();
        }

        public IDatabaseCommand CreateCommand(object query) {
            SQLiteCommand comm = Connection.CreateCommand();
            comm.CommandText = query.ToString();
            comm.CommandTimeout = 5;
            return new SQLiteDatabaseCommand(this, comm);
        }

        private bool IsDisposed;

        ~SQLiteDatabaseConnection()
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
