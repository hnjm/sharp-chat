using System;
using System.IO;
using System.Text;
using System.Threading;

namespace SharpChat.Configuration {
    public class StreamConfig : IConfig {
        private Stream Stream { get; }
        private StreamReader StreamReader { get; }
        private Mutex Lock { get; }

        private const int LOCK_TIMEOUT = 10000;

        private static readonly TimeSpan CACHE_LIFETIME = TimeSpan.FromMinutes(15);

        public StreamConfig(string fileName)
            : this(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite)) {}

        public StreamConfig(Stream stream) {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if(!Stream.CanRead)
                throw new ArgumentException(@"Provided stream must be readable.", nameof(stream));
            if(!Stream.CanSeek)
                throw new ArgumentException(@"Provided stream must be seekable.", nameof(stream));
            StreamReader = new StreamReader(stream, new UTF8Encoding(false), false);
            Lock = new Mutex();
        }

        public string ReadValue(string name, string fallback = null) {
            if(!Lock.WaitOne(LOCK_TIMEOUT)) // don't catch this, if this happens something is Very Wrong
                throw new ConfigLockException();

            try {
                Stream.Seek(0, SeekOrigin.Begin);

                string line;
                while((line = StreamReader.ReadLine()) != null) {
                    if(string.IsNullOrWhiteSpace(line))
                        continue;

                    line = line.TrimStart();
                    if(line.StartsWith(@";") || line.StartsWith(@"#"))
                        continue;

                    string[] parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if(parts.Length < 2 || !string.Equals(parts[0], name))
                        continue;

                    return parts[1];
                }
            } finally {
                Lock.ReleaseMutex();
            }

            return fallback;
        }

        public T ReadValue<T>(string name, T fallback = default) {
            object value = ReadValue(name);
            if(value == null)
                return fallback;

            Type type = typeof(T);
            if(value is string strVal) {
                if(type == typeof(bool))
                    value = !string.Equals(strVal, @"0", StringComparison.InvariantCultureIgnoreCase)
                        && !string.Equals(strVal, @"false", StringComparison.InvariantCultureIgnoreCase);
                else if(type == typeof(string[]))
                    value = strVal.Split(' ');
            }

            try {
                return (T)Convert.ChangeType(value, type);
            } catch(InvalidCastException ex) {
                throw new ConfigTypeException(ex);
            }
        }

        public T SafeReadValue<T>(string name, T fallback) {
            try {
                return ReadValue(name, fallback);
            } catch(ConfigTypeException) {
                return fallback;
            }
        }

        public IConfig ScopeTo(string prefix) {
            return new ScopedConfig(this, prefix);
        }
        
        public CachedValue<T> ReadCached<T>(string name, T fallback = default, TimeSpan? lifetime = null) {
            return new CachedValue<T>(this, name, lifetime ?? CACHE_LIFETIME, fallback);
        }

        private bool IsDisposed;
        ~StreamConfig()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;

            StreamReader.Dispose();
            Stream.Dispose();
            Lock.Dispose();
        }
    }
}
