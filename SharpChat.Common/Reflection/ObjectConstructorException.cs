using System;

namespace SharpChat.Reflection {
    public class ObjectConstructorException : Exception {
        public ObjectConstructorException(string message) : base(message) {
        }
    }

    public class ObjectConstructorObjectNotFoundException : ObjectConstructorException {
        public ObjectConstructorObjectNotFoundException(string name) : base($@"Object with name {name} not found.") { }
    }

    public class ObjectConstructorConstructorNotFoundException : ObjectConstructorException {
        public ObjectConstructorConstructorNotFoundException(string name) : base($@"Proper constructor for object {name} not found.") { }
    }
}
