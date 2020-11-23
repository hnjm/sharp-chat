using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;

namespace SharpChat.Database.MariaDB {
    public class MariaDBDatabaseCommand : IDatabaseCommand {
        public IDatabaseConnection Connection { get; }
        private MySqlCommand Command { get; }

        public string CommandString => Command.CommandText;

        public MariaDBDatabaseCommand(MariaDBDatabaseConnection connection, MySqlCommand command) {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public IDatabaseParameter AddParameter(string name, object value) {
            return new MariaDBDatabaseParameter(Command.Parameters.AddWithValue(name, value));
        }

        public IDatabaseParameter AddParameter(IDatabaseParameter param) {
            if(param is not MariaDBDatabaseParameter mdbParam)
                throw new InvalidParameterClassTypeException();
            Command.Parameters.Add(mdbParam.Parameter);
            return mdbParam;
        }

        public void AddParameters(IDatabaseParameter[] @params) {
            Command.Parameters.AddRange(@params.OfType<MariaDBDatabaseParameter>().Select(x => x.Parameter).ToArray());
        }

        public void ClearParameters() {
            Command.Parameters.Clear();
        }

        public void Prepare() {
            Command.Prepare();
        }

        public int Execute()
            => Command.ExecuteNonQuery();

        public IDatabaseReader ExecuteReader()
            => new ADODatabaseReader(Command.ExecuteReader());

        public object ExecuteScalar()
            => Command.ExecuteScalar();

        private bool IsDisposed;
        ~MariaDBDatabaseCommand()
            => Dispose(false);
        public void Dispose()
            => Dispose(true);
        private void Dispose(bool disposing) {
            if(IsDisposed)
                return;
            IsDisposed = true;
            
            Command.Dispose();

            if(disposing)
                GC.SuppressFinalize(this);
        }
    }
}
