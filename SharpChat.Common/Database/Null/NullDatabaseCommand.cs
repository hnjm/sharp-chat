using System;

namespace SharpChat.Database.Null {
    public class NullDatabaseCommand : IDatabaseCommand {
        public IDatabaseConnection Connection { get; }

        public string CommandString => string.Empty;

        public NullDatabaseCommand(NullDatabaseConnection conn) {
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        public IDatabaseParameter AddParameter(string name, object value) {
            return new NullDatabaseParameter();
        }

        public IDatabaseParameter AddParameter(IDatabaseParameter param) {
            if(param is not NullDatabaseParameter)
                throw new InvalidParameterClassTypeException();
            return param;
        }

        public void AddParameters(IDatabaseParameter[] @params) {}
        public void ClearParameters() {}

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public int Execute() {
            return 0;
        }

        public IDatabaseReader ExecuteReader() {
            return new NullDatabaseReader();
        }

        public object ExecuteScalar() {
            return null;
        }

        public void Prepare() {}
    }
}
