namespace SharpChat.Database {
    public interface IDatabaseParameter {
        string Name { get; }
        object Value { get; }
    }
}
