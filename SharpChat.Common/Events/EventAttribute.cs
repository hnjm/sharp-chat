using System;

namespace SharpChat.Events {
    [AttributeUsage(AttributeTargets.Class)]
    public class EventAttribute : Attribute {
        public string Type { get; }

        public EventAttribute(string type) {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}
