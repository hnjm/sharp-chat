using SharpChat.Reflection;

namespace SharpChat.DataProvider {
    public class DataProviderAttribute : ObjectConstructorAttribute {
        public DataProviderAttribute(string name) : base(name) {
        }
    }
}
