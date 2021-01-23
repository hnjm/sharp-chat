using System;

namespace SharpChat.Configuration {
    public abstract class ConfigException : Exception {
        public ConfigException(string message) : base(message) { }
    }

    public class ConfigLockException : ConfigException {
        public ConfigLockException() : base(@"Unable to acquire lock for reading configuration.") { }
    }
}
