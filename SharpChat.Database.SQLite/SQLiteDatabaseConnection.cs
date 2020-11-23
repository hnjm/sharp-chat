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
