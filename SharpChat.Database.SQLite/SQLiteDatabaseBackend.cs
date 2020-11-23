using System;
using System.Data.SQLite;

namespace SharpChat.Database.SQLite {
    public class SQLiteDatabaseBackend : IDatabaseBackend {
        private string DSN { get; }

        public SQLiteDatabaseBackend(string path) {
            DSN = new SQLiteConnectionStringBuilder {
                DataSource = path,
                DateTimeFormat = SQLiteDateFormats.ISO8601,
                DateTimeKind = DateTimeKind.Utc,
                ForeignKeys = true,
                LegacyFormat = false,
                Pooling = true,
                Version = 3,
            }.ToString();
        }

        public IDatabaseConnection CreateConnection()
            => new SQLiteDatabaseConnection(DSN);

        public IDatabaseParameter CreateParameter(string name, object value)
            => new SQLiteDatabaseParameter(name, value);

        public string FromUnixTime(string param)
            => string.Format(@"datetime({0}, 'unixepoch')", param);
        public string ToUnixTime(string param)
            => string.Format(@"strftime('%s', {0})", param);
        public string DateTimeNow()
            => @"datetime('now')";

        private bool IsDisposed;
        ~SQLiteDatabaseBackend()
            => Dispose(false);
        public void Dispose()
            => Dispose(true);
        private void Dispose(bool disposing) {
            if(IsDisposed)
                return;
            IsDisposed = true;

            if(disposing)
                GC.SuppressFinalize(this);
        }
    }
}
