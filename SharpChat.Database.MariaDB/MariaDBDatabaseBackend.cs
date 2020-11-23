using MySql.Data.MySqlClient;
using System;

namespace SharpChat.Database.MariaDB {
    public class MariaDBDatabaseBackend : IDatabaseBackend {
        private string DSN { get; }

        public MariaDBDatabaseBackend(string host, string username, string password, string database, string charset = @"utf8mb4") {
            DSN = new MySqlConnectionStringBuilder {
                Server = host,
                UserID = username,
                Password = password,
                Database = database,
                IgnorePrepare = false,
                OldGuids = false,
                TreatTinyAsBoolean = false,
                CharacterSet = charset,
                TreatBlobsAsUTF8 = false
            }.ToString();
        }

        public IDatabaseConnection CreateConnection()
            => new MariaDBDatabaseConnection(DSN);

        public IDatabaseParameter CreateParameter(string name, object value)
            => new MariaDBDatabaseParameter(name, value);

        public string FromUnixTime(string param)
            => string.Format(@"FROM_UNIXTIME({0})", param);
        public string ToUnixTime(string param)
            => string.Format(@"UNIX_TIMESTAMP({0})", param);
        public string DateTimeNow()
            => @"NOW()";

        private bool IsDisposed;
        ~MariaDBDatabaseBackend()
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
