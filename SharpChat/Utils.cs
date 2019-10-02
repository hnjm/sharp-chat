using System.IO;

namespace SharpChat
{
    public static class Utils
    {
        public static string ReadFileOrDefault(string file, string def)
            => File.Exists(file) ? File.ReadAllText(file) : def;
    }
}
