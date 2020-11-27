using System;

namespace SharpChat.Database.Null {
    public class NullDatabaseConnection : IDatabaseConnection {
        public IDatabaseCommand CreateCommand(object query) {
            return new NullDatabaseCommand(this);
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }
}
