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

        private bool IsDisposed;
        ~MariaDBDatabaseBackend()
            => Dispose(false);
        public void Dispose()
            => Dispose(true);
        private void Dispose(bool disposing) {
            if(IsDisposed)
                return;

            if(disposing)
                GC.SuppressFinalize(this);
        }
    }
}
