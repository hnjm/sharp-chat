using System;

namespace SharpChat.Configuration {
    public class ScopedConfig : IConfig {
        private IConfig Config { get; }
        private string Prefix { get; }

        public ScopedConfig(IConfig config, string prefix) {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            if(string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException(@"Prefix must exist.", nameof(prefix));
            if(Prefix[^1] != ':')
                Prefix += ':';
        }

        private string GetName(string name) {
            return Prefix + name;
        }

        public string ReadValue(string name, string fallback = null) {
            return Config.ReadValue(GetName(name), fallback);
        }

        public T ReadValue<T>(string name, T fallback = default) {
            return Config.ReadValue(GetName(name), fallback);
        }

        public IConfig ScopeTo(string name) {
            return Config.ScopeTo(GetName(name));
        }

        public CachedValue<T> ReadCached<T>(string name, T fallback = default, TimeSpan? lifetime = null) {
            return Config.ReadCached(GetName(name), fallback, lifetime);
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }
}
