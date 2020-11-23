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

        public string FromUnixTime(string param)
            => string.Empty;
        public string ToUnixTime(string param)
            => string.Empty;
        public string DateTimeNow()
            => string.Empty;
    }
}
