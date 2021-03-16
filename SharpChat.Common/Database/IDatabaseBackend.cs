using System.Collections.Generic;

namespace SharpChat.Database {
    public interface IDatabaseBackend {
        IDatabaseConnection CreateConnection();

        IDatabaseParameter CreateParameter(string name, object value);
        IDatabaseParameter CreateParameter(string name, DatabaseType type);

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

        string Concat(params string[] args);
        string ToLower(string param);

        bool SupportsJson { get; }
        string JsonSet(string field, string path, string value);
        string JsonSet(string field, IDictionary<string, object> values);

        bool SupportsAlterTableCollate { get; }

        string AsciiCollation { get; }
        string UnicodeCollation { get; }
    }
}
