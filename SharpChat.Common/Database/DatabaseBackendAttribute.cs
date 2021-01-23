using System;

namespace SharpChat.Database {
    [AttributeUsage(AttributeTargets.Class)]
    public class DatabaseBackendAttribute : Attribute {
        public string Name { get; }

        public DatabaseBackendAttribute(string name) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
