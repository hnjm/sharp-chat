using System;
using System.Threading;

namespace SharpChat.Configuration {
    public class CachedValue<T> : IDisposable {
        private IConfig Config { get; }
        private string Name { get; }
        private TimeSpan Lifetime { get; }
        private T Fallback { get; }
        private Mutex Lock { get; }

        private object CurrentValue { get; set; }
        private DateTimeOffset LastRead { get; set; }

        private const int LOCK_TIMEOUT = 10000;

        public T Value {
            get {
                if(!Lock.WaitOne(LOCK_TIMEOUT))
                    throw new ConfigLockException();

                try {
                    DateTimeOffset now = DateTimeOffset.Now;
                    if((now - LastRead) >= Lifetime) {
                        LastRead = now;
                        CurrentValue = Config.ReadValue(Name, Fallback);
                        Logger.Debug($@"Read {Name} ({CurrentValue})");
                    }
                } finally {
                    Lock.ReleaseMutex();
                }

                return (T)CurrentValue;
            }
        }

        public static implicit operator T(CachedValue<T> val) => val.Value;

        public CachedValue(IConfig config, string name, TimeSpan lifetime, T fallback) {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Lifetime = lifetime;
            Fallback = fallback;
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(@"Name cannot be empty.", nameof(name));
            Lock = new Mutex();
        }

        private bool IsDisposed;
        ~CachedValue()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;
            Lock.Dispose();
        }
    }
}
