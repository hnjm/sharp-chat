using System;
using System.Data.SQLite;
using System.Linq;

namespace SharpChat.Database.SQLite {
    public class SQLiteDatabaseCommand : IDatabaseCommand {
        public IDatabaseConnection Connection { get; }
        private SQLiteCommand Command { get; }

        public string CommandString => Command.CommandText;

        public SQLiteDatabaseCommand(SQLiteDatabaseConnection conn, SQLiteCommand comm) {
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
            Command = comm ?? throw new ArgumentNullException(nameof(comm));
        }

        public IDatabaseParameter AddParameter(string name, object value) {
            return new SQLiteDatabaseParameter(Command.Parameters.AddWithValue(name, value));
        }

        public IDatabaseParameter AddParameter(IDatabaseParameter param) {
            if(param is not SQLiteDatabaseParameter sqlParam)
                throw new InvalidParameterClassTypeException();
            Command.Parameters.Add(sqlParam.Parameter);
            return sqlParam;
        }

        public void AddParameters(IDatabaseParameter[] @params) {
            Command.Parameters.AddRange(@params.OfType<SQLiteDatabaseParameter>().Select(x => x.Parameter).ToArray());
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
        ~SQLiteDatabaseCommand()
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
