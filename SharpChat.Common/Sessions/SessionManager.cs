using SharpChat.Configuration;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Sessions {
    public class SessionManager {
        public const short DEFAULT_MAX_COUNT = 5;
        public const ushort DEFAULT_TIMEOUT = 5;

        private object Sync { get; } = new object();

        private CachedValue<short> MaxPerUser { get; } 
        private CachedValue<ushort> TimeOut { get; }

        private List<Session> Sessions { get; } = new List<Session>();

        public SessionManager(IConfig config) {
            MaxPerUser = config.ReadCached(@"maxCount", DEFAULT_MAX_COUNT);
            TimeOut = config.ReadCached(@"timeOut", DEFAULT_TIMEOUT);
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

            lock(Sync) {
                Sessions.Add(session);
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

            lock(Sync) {
                return Sessions.FirstOrDefault(predicate);
            }
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

            lock(Sync) {
                callback.Invoke(Sessions.Where(predicate));
            }
        }

        public Session ByConnection(IConnection connection) {
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
    }
}
