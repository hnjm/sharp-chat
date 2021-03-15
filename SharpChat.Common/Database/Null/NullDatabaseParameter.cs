namespace SharpChat.Database.Null {
    public class NullDatabaseParameter : IDatabaseParameter {
        public string Name => string.Empty;
        public object Value { get => null; set { } }
    }
}
