using System;
using System.Data.Common;

namespace SharpChat.Database {
    public class ADODatabaseReader : IDatabaseReader {
        private DbDataReader Reader { get; }

        public ADODatabaseReader(DbDataReader reader) {
            Reader = reader;
        }

        public bool Next()
            => Reader.Read();

        public string GetName(int ordinal)
            => Reader.GetName(ordinal);
        public int GetOrdinal(string name)
            => Reader.GetOrdinal(name);

        public bool IsNull(int ordinal)
            => Reader.IsDBNull(ordinal);
        public bool IsNull(string name)
            => Reader.IsDBNull(GetOrdinal(name));

        public object GetValue(int ordinal)
            => Reader.GetValue(ordinal);
        public object GetValue(string name)
            => Reader.GetValue(GetOrdinal(name));

        public string ReadString(int ordinal)
            => Reader.GetString(ordinal);
        public string ReadString(string name)
            => Reader.GetString(GetOrdinal(name));

        public byte ReadU8(int ordinal)
            => Reader.GetByte(ordinal);
        public byte ReadU8(string name)
            => Reader.GetByte(GetOrdinal(name));

        public short ReadI16(int ordinal)
            => Reader.GetInt16(ordinal);
        public short ReadI16(string name)
            => Reader.GetInt16(GetOrdinal(name));

        public int ReadI32(int ordinal)
            => Reader.GetInt32(ordinal);
        public int ReadI32(string name)
            => Reader.GetInt32(GetOrdinal(name));

        public long ReadI64(int ordinal)
            => Reader.GetInt64(ordinal);
        public long ReadI64(string name)
            => Reader.GetInt64(GetOrdinal(name));

        public float ReadF32(int ordinal)
            => Reader.GetFloat(ordinal);
        public float ReadF32(string name)
            => Reader.GetFloat(GetOrdinal(name));

        public double ReadF64(int ordinal)
            => Reader.GetDouble(ordinal);
        public double ReadF64(string name)
            => Reader.GetDouble(GetOrdinal(name));

        private bool IsDisposed;
        ~ADODatabaseReader()
            => Dispose(false);
        public void Dispose()
            => Dispose(true);
        private void Dispose(bool disposing) {
            if(IsDisposed)
                return;
            IsDisposed = true;

            if(Reader is IDisposable disposable)
                disposable.Dispose();

            if(disposing)
                GC.SuppressFinalize(this);
        }
    }
}
