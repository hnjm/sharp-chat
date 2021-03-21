using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Database {
    public class DatabaseWrapper {
        private IDatabaseBackend Backend { get; }

        public bool IsNullBackend
            => Backend is Null.NullDatabaseBackend;

        public DatabaseWrapper(IDatabaseBackend backend) {
            Backend = backend ?? throw new ArgumentNullException(nameof(backend));
        }

        public IDatabaseParameter CreateParam(string name, object value)
            => Backend.CreateParameter(name, value);

        public string TimestampType
            => Backend.TimestampType;
        public string TextType
            => Backend.TextType;
        public string BlobType
            => Backend.BlobType;
        public string VarCharType(int size)
            => Backend.VarCharType(size);
        public string VarBinaryType(int size)
            => Backend.VarBinaryType(size);
        public string BigIntType(int length)
            => Backend.BigIntType(length);
        public string BigUIntType(int length)
            => Backend.BigUIntType(length);
        public string IntType(int length)
            => Backend.IntType(length);
        public string UIntType(int length)
            => Backend.UIntType(length);
        public string TinyIntType(int length)
            => Backend.TinyIntType(length);
        public string TinyUIntType(int length)
            => Backend.TinyUIntType(length);

        public string ToUnixTime(string param)
            => Backend.ToUnixTime(param);
        public string FromUnixTime(string param)
            => Backend.FromUnixTime(param);
        public string DateTimeNow()
            => Backend.DateTimeNow();

        public string Concat(params string[] args)
            => Backend.Concat(args);
        public string ToLower(string param)
            => Backend.ToLower(param);

        public bool SupportsJson
            => Backend.SupportsJson;
        public string JsonValue(string field, string path)
            => Backend.JsonValue(field, path);

        public bool SupportsAlterTableCollate
            => Backend.SupportsAlterTableCollate;

        public string AsciiCollation
            => Backend.AsciiCollation;
        public string UnicodeCollation
            => Backend.UnicodeCollation;

        public void RunCommand(object query, int timeout, Action<IDatabaseCommand> action, params IDatabaseParameter[] @params) {
#if LOG_SQL
            Logger.Debug(query);
#endif
            using IDatabaseConnection conn = Backend.CreateConnection();
            using IDatabaseCommand comm = conn.CreateCommand(query);
            comm.CommandTimeout = timeout;
            if(@params.Any()) {
                comm.AddParameters(@params);
                comm.Prepare();
            }
            action.Invoke(comm);
        }

        public void RunCommand(object query, Action<IDatabaseCommand> action, params IDatabaseParameter[] @params)
            => RunCommand(query, 30, action, @params);

        public int RunCommand(object query, params IDatabaseParameter[] @params) {
            int affected = 0;
            RunCommand(query, comm => affected = comm.Execute(), @params);
            return affected;
        }

        public int RunCommand(object query, int timeout, params IDatabaseParameter[] @params) {
            int affected = 0;
            RunCommand(query, timeout, comm => affected = comm.Execute(), @params);
            return affected;
        }

        public object RunQueryValue(object query, params IDatabaseParameter[] @params) {
            object value = null;
            RunCommand(query, comm => value = comm.ExecuteScalar(), @params);
            return value;
        }

        public void RunQuery(object query, Action<IDatabaseReader> action, params IDatabaseParameter[] @params) {
            RunCommand(query, comm => {
                using IDatabaseReader reader = comm.ExecuteReader();
                action.Invoke(reader);
            }, @params);
        }
    }
}
