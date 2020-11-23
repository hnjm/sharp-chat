using System;

namespace SharpChat.Database.Null {
    public class NullDatabaseBackend : IDatabaseBackend {
        public IDatabaseConnection CreateConnection() {
            return new NullDatabaseConnection();
        }

        public IDatabaseParameter CreateParameter(string name, object value) {
            return new NullDatabaseParameter();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }
}
