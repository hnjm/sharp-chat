using MySql.Data.MySqlClient;
using System;

namespace SharpChat.Database.MariaDB {
    public class MariaDBDatabaseParameter : IDatabaseParameter {
        public MySqlParameter Parameter { get; }

        public string Name => Parameter.ParameterName;
        public object Value { get => Parameter.Value; set => Parameter.Value = value; }

        public MariaDBDatabaseParameter(string name, object value) : this(new MySqlParameter(name, value)) { }
        public MariaDBDatabaseParameter(string name, DatabaseType type) : this(new MySqlParameter(name, MapType(type))) { }

        public MariaDBDatabaseParameter(MySqlParameter parameter) {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }

        internal static MySqlDbType MapType(DatabaseType type) {
            return type switch {
                DatabaseType.AsciiString => MySqlDbType.VarBinary,
                DatabaseType.UnicodeString => MySqlDbType.VarString,
                DatabaseType.Int8 => MySqlDbType.Byte,
                DatabaseType.Int16 => MySqlDbType.Int16,
                DatabaseType.Int32 => MySqlDbType.Int32,
                DatabaseType.Int64 => MySqlDbType.Int64,
                DatabaseType.UInt8 => MySqlDbType.UByte,
                DatabaseType.UInt16 => MySqlDbType.UInt16,
                DatabaseType.UInt32 => MySqlDbType.UInt32,
                DatabaseType.UInt64 => MySqlDbType.UInt64,
                _ => throw new ArgumentException($@"Unsupported type {type}.", nameof(type)),
            };
        }
    }
}
