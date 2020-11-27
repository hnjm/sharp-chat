using System;
using System.Data.SQLite;

namespace SharpChat.Database.SQLite {
    public class SQLiteDatabaseParameter : IDatabaseParameter {
        public SQLiteParameter Parameter { get; }

        public string Name => Parameter.ParameterName;
        public object Value => Parameter.Value;

        public SQLiteDatabaseParameter(string name, object value) : this(new SQLiteParameter(name, value)) { }

        public SQLiteDatabaseParameter(SQLiteParameter parameter) {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }
    }
}
