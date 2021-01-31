using SharpChat.Configuration;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpChat.Sessions {
    public class SessionManager : IDisposable {
        public const short DEFAULT_MAX_COUNT = 5;
        public const ushort DEFAULT_TIMEOUT = 5;

        private Mutex Sync { get; }

        private CachedValue<short> MaxPerUser { get; } 
        private CachedValue<ushort> TimeOut { get; }

        private List<Session> Sessions { get; } = new List<Session>();

        public SessionManager(IConfig config) {
            MaxPerUser = config.ReadCached(@"maxCount", DEFAULT_MAX_COUNT);
            TimeOut = config.ReadCached(@"timeOut", DEFAULT_TIMEOUT);

            Sync = new Mutex();
        }

        public bool HasTimedOut(Session session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            int timeOut = TimeOut;
            if(timeOut < 1) // avoid idiocy
                timeOut = DEFAULT_TIMEOUT;
            return session.IdleTime.TotalSeconds >= timeOut;
        }

        public void Add(Session session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            Sync.WaitOne();
            try {
                Sessions.Add(session);
            } finally {
                Sync.ReleaseMutex();
            }
        }

        public int GetSessionCount(IUser user) {
            int count = 0;
            FindMany(s => s.User == user, s => count = s.Count());
            return count;
        }

        public int GetAvailableSessionCount(IUser user) {
            return MaxPerUser - GetSessionCount(user);
        }

        public Session Find(Func<Session, bool> predicate) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            Session session = null;

            Sync.WaitOne();
            try {
                session = Sessions.FirstOrDefault(predicate);
            } finally {
                Sync.ReleaseMutex();
            }

            return session;
        }

        public IEnumerable<Session> FindMany(Func<Session, bool> predicate) {
            IEnumerable<Session> sessions = null;
            FindMany(predicate, s => sessions = s.ToArray());
            return sessions ?? Enumerable.Empty<Session>();
        }

        public void FindMany(Func<Session, bool> predicate, Action<IEnumerable<Session>> callback) {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            Sync.WaitOne();
            try {
                callback.Invoke(Sessions.Where(predicate));
            } finally {
                Sync.ReleaseMutex();
            }
        }

        public Session ByConnection(IWebSocketConnection connection) {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            return Find(c => c.Connection == connection);
        }

        public IEnumerable<Session> ByUser(IUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            return FindMany(s => s.User == user);
        }

        public void DisposeTimedOut() {
            FindMany(s => HasTimedOut(s), sessions => {
                Session session;
                while((session = sessions.FirstOrDefault()) != null) {
                    session.Dispose();
                    Sessions.Remove(session);
                }
            });
        }

        private bool IsDisposed;
        ~SessionManager()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;

            Sync.Dispose();
        }
    }
}
