using System;

namespace SharpChat.Configuration {
    public interface IConfig : IDisposable {
        IConfig ScopeTo(string name);
        string ReadValue(string name, string fallback = null);
        T ReadValue<T>(string name, T fallback = default);
        CachedValue<T> ReadCached<T>(string name, T fallback = default, TimeSpan? lifetime = null);
    }
}
