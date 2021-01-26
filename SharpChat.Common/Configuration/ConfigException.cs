using System;

namespace SharpChat.Configuration {
    public abstract class ConfigException : Exception {
        public ConfigException(string message) : base(message) { }
        public ConfigException(string message, Exception ex) : base(message, ex) { }
    }

    public class ConfigLockException : ConfigException {
        public ConfigLockException() : base(@"Unable to acquire lock for reading configuration.") { }
    }

    public class ConfigTypeException : ConfigException {
        public ConfigTypeException(Exception ex) : base(@"Given type does not match the value in the configuration.", ex) { }
    }
}
