using System;

namespace SharpChat.Database {
    public interface IDatabaseReader : IDisposable {
        bool Next();

        object GetValue(int ordinal);
        object GetValue(string name);

        bool IsNull(int ordinal);
        bool IsNull(string name);

        string GetName(int ordinal);
        int GetOrdinal(string name);

        string ReadString(int ordinal);
        string ReadString(string name);

        byte ReadU8(int ordinal);
        byte ReadU8(string name);

        short ReadI16(int ordinal);
        short ReadI16(string name);

        int ReadI32(int ordinal);
        int ReadI32(string name);

        long ReadI64(int ordinal);
        long ReadI64(string name);

        float ReadF32(int ordinal);
        float ReadF32(string name);

        double ReadF64(int ordinal);
        double ReadF64(string name);
    }
}
