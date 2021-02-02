using SharpChat.Reflection;

namespace SharpChat.Database {
    public class DatabaseBackendAttribute : ObjectConstructorAttribute {
        public DatabaseBackendAttribute(string name) : base(name) {
        }
    }
}
