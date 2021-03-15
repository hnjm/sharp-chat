using System;
using System.Data;
using System.Data.SQLite;

namespace SharpChat.Database.SQLite {
    public class SQLiteDatabaseParameter : IDatabaseParameter {
        public SQLiteParameter Parameter { get; }

        public string Name => Parameter.ParameterName;
        public object Value { get => Parameter.Value; set => Parameter.Value = value; }

        public SQLiteDatabaseParameter(string name, object value) : this(new SQLiteParameter(name, value)) { }
        public SQLiteDatabaseParameter(string name, DatabaseType type) : this(new SQLiteParameter(name, MapType(type))) { }

        public SQLiteDatabaseParameter(SQLiteParameter parameter) {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }

        internal static DbType MapType(DatabaseType type) {
            return type switch {
                DatabaseType.AsciiString => DbType.AnsiString,
                DatabaseType.UnicodeString => DbType.String,
                DatabaseType.Int8 => DbType.SByte,
                DatabaseType.Int16 => DbType.Int16,
                DatabaseType.Int32 => DbType.Int32,
                DatabaseType.Int64 => DbType.Int64,
                DatabaseType.UInt8 => DbType.Byte,
                DatabaseType.UInt16 => DbType.UInt16,
                DatabaseType.UInt32 => DbType.UInt32,
                DatabaseType.UInt64 => DbType.UInt64,
                _ => throw new ArgumentException($@"Unsupported type {type}.", nameof(type)),
            };
        }
    }
}
