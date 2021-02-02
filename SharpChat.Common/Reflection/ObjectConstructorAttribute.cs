using System;

namespace SharpChat.Reflection {
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class ObjectConstructorAttribute : Attribute {
        public string Name { get; }

        public ObjectConstructorAttribute(string name) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
