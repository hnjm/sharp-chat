using System;

namespace SharpChat.Database.Null {
    public class NullDatabaseReader : IDatabaseReader {
        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public string GetName(int ordinal) {
            return string.Empty;
        }

        public int GetOrdinal(string name) {
            return 0;
        }

        public object GetValue(int ordinal) {
            return null;
        }

        public object GetValue(string name) {
            return null;
        }

        public bool IsNull(int ordinal) {
            return true;
        }
        public bool IsNull(string name) {
            return true;
        }

        public bool Next() {
            return false;
        }

        public float ReadF32(int ordinal) {
            return 0f;
        }

        public float ReadF32(string name) {
            return 0f;
        }

        public double ReadF64(int ordinal) {
            return 0d;
        }

        public double ReadF64(string name) {
            return 0d;
        }

        public short ReadI16(int ordinal) {
            throw new NotImplementedException();
        }

        public short ReadI16(string name) {
            return 0;
        }

        public int ReadI32(int ordinal) {
            return 0;
        }

        public int ReadI32(string name) {
            return 0;
        }

        public long ReadI64(int ordinal) {
            return 0;
        }

        public long ReadI64(string name) {
            return 0;
        }

        public sbyte ReadI8(int ordinal) {
            return 0;
        }

        public sbyte ReadI8(string name) {
            return 0;
        }

        public string ReadString(int ordinal) {
            return string.Empty;
        }

        public string ReadString(string name) {
            return string.Empty;
        }

        public ushort ReadU16(int ordinal) {
            return 0;
        }

        public ushort ReadU16(string name) {
            return 0;
        }

        public uint ReadU32(int ordinal) {
            return 0;
        }

        public uint ReadU32(string name) {
            return 0;
        }

        public ulong ReadU64(int ordinal) {
            return 0;
        }

        public ulong ReadU64(string name) {
            return 0;
        }

        public byte ReadU8(int ordinal) {
            return 0;
        }

        public byte ReadU8(string name) {
            return 0;
        }
    }
}
