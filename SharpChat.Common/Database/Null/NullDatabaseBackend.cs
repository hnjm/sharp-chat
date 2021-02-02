﻿using SharpChat.Configuration;

namespace SharpChat.Database.Null {
    [DatabaseBackend(@"null")]
    public class NullDatabaseBackend : IDatabaseBackend {
        public NullDatabaseBackend(IConfig _ = null) { }

        public IDatabaseConnection CreateConnection() {
            return new NullDatabaseConnection();
        }

        public IDatabaseParameter CreateParameter(string name, object value) {
            return new NullDatabaseParameter();
        }

        public string TimestampType
            => string.Empty;
        public string BlobType
            => string.Empty;

        public string VarCharType(int size)
            => string.Empty;
        public string VarBinaryType(int size)
            => string.Empty;
        public string BigIntType(int length)
            => string.Empty;
        public string BigUIntType(int length)
            => string.Empty;
        public string IntType(int length)
            => string.Empty;
        public string UIntType(int length)
            => string.Empty;
        public string TinyIntType(int length)
            => string.Empty;
        public string TinyUIntType(int length)
            => string.Empty;

        public string FromUnixTime(string param)
            => string.Empty;
        public string ToUnixTime(string param)
            => string.Empty;
        public string DateTimeNow()
            => string.Empty;

        public string Concat(params string[] args)
            => string.Empty;
        public string ToLower(string param)
            => string.Empty;

        public bool SupportsAlterTableCollate => true;

        public string AsciiCollation => string.Empty;
        public string UnicodeCollation => string.Empty;
    }
}
