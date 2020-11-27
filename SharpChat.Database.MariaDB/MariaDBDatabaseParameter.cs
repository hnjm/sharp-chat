using MySql.Data.MySqlClient;
using System;

namespace SharpChat.Database.MariaDB {
    public class MariaDBDatabaseParameter : IDatabaseParameter {
        public MySqlParameter Parameter { get; }

        public string Name => Parameter.ParameterName;
        public object Value => Parameter.Value;

        public MariaDBDatabaseParameter(string name, object value) : this(new MySqlParameter(name, value)) {}

        public MariaDBDatabaseParameter(MySqlParameter parameter) {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }
    }
}
