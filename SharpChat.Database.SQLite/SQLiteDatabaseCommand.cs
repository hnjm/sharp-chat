using System;
using System.Data.SQLite;
using System.Linq;

namespace SharpChat.Database.SQLite {
    public class SQLiteDatabaseCommand : IDatabaseCommand {
        public IDatabaseConnection Connection { get; }
        private SQLiteCommand Command { get; }

        public string CommandString => Command.CommandText;
        public int CommandTimeout { get => Command.CommandTimeout; set => Command.CommandTimeout = value; }

        public SQLiteDatabaseCommand(SQLiteDatabaseConnection conn, SQLiteCommand comm) {
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
            Command = comm ?? throw new ArgumentNullException(nameof(comm));
        }

        public IDatabaseParameter AddParameter(string name, object value)
            => new SQLiteDatabaseParameter(Command.Parameters.AddWithValue(name, value));

        public IDatabaseParameter AddParameter(string name, DatabaseType type) {
            SQLiteParameter param = Command.CreateParameter();
            param.ParameterName = name;
            param.DbType = SQLiteDatabaseParameter.MapType(type);
            return new SQLiteDatabaseParameter(param);
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
            => DoDispose();

        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }

        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;
            Command.Dispose();
        }
    }
}
