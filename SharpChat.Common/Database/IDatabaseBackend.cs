namespace SharpChat.Database {
    public interface IDatabaseBackend {
        IDatabaseConnection CreateConnection();
        IDatabaseParameter CreateParameter(string name, object value);

        string TimestampType { get; }
        string BlobType { get; }
        string VarCharType(int length);
        string VarBinaryType(int length);
        string BigIntType(int length);
        string BigUIntType(int length);
        string IntType(int length);
        string UIntType(int length);
        string TinyIntType(int length);
        string TinyUIntType(int length);

        string FromUnixTime(string param);
        string ToUnixTime(string param);
        string DateTimeNow();

        bool SupportsAlterTableCollate { get; }

        string AsciiCollation { get; }
        string UnicodeCollation { get; }
    }
}
