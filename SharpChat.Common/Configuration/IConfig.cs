using System;

namespace SharpChat.Configuration {
    public interface IConfig : IDisposable {
        /// <summary>
        /// Creates a proxy object that forces all names to start with the given prefix.
        /// </summary>
        IConfig ScopeTo(string prefix);

        /// <summary>
        /// Reads a raw (string) value from the config.
        /// </summary>
        string ReadValue(string name, string fallback = null);

        /// <summary>
        /// Reads and casts value from the config.
        /// </summary>
        /// <exception cref="ConfigTypeException">Type conversion failed.</exception>
        T ReadValue<T>(string name, T fallback = default);

        /// <summary>
        /// Reads and casts a value from the config. Returns fallback when type conversion fails.
        /// </summary>
        T SafeReadValue<T>(string name, T fallback);

        /// <summary>
        /// Creates an object that caches the read value for a certain amount of time, avoiding disk reads for frequently used non-static values.
        /// </summary>
        CachedValue<T> ReadCached<T>(string name, T fallback = default, TimeSpan? lifetime = null);
    }
}
