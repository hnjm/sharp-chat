using System;

namespace SharpChat.Database {
    public interface IDatabaseCommand : IDisposable {
        IDatabaseConnection Connection { get; }

        string CommandString { get; }

        IDatabaseParameter AddParameter(string name, object value);
        IDatabaseParameter AddParameter(IDatabaseParameter param);
        void AddParameters(IDatabaseParameter[] @params);
        void ClearParameters();
        void Prepare();

        int Execute();
        IDatabaseReader ExecuteReader();
        object ExecuteScalar();
    }
}
