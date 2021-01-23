using System;

namespace SharpChat.DataProvider {
    [AttributeUsage(AttributeTargets.Class)]
    public class DataProviderAttribute : Attribute {
        public string Name { get; }

        public DataProviderAttribute(string name) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
