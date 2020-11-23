using System;

namespace SharpChat.Database {
    public interface IDatabaseConnection : IDisposable {
        IDatabaseCommand CreateCommand(object query);
    }
}
