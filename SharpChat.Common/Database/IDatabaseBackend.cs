using System;

namespace SharpChat.Database {
    public interface IDatabaseBackend : IDisposable {
        IDatabaseConnection CreateConnection();
        IDatabaseParameter CreateParameter(string name, object value);

        string FromUnixTime(string param);
        string ToUnixTime(string param);
        string DateTimeNow();
    }
}
