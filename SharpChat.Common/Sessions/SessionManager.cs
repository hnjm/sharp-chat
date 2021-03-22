using SharpChat.Configuration;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Sessions {
    public class SessionManager : IEventHandler {
        public const short DEFAULT_MAX_COUNT = 5;
        public const ushort DEFAULT_TIMEOUT = 5;

        private readonly object Sync = new object();

        private CachedValue<short> MaxPerUser { get; } 
        private CachedValue<ushort> TimeOut { get; }

        private IEventDispatcher Dispatcher { get; }
        private string ServerId { get; }

        private List<ISession> Sessions { get; } = new List<ISession>();
        private List<ILocalSession> LocalSessions { get; } = new List<ILocalSession>();

        public SessionManager(IEventDispatcher dispatcher, string serverId, IConfig config) {
            if(config == null)
                throw new ArgumentNullException(nameof(config));
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            ServerId = serverId ?? throw new ArgumentNullException(nameof(serverId));
            MaxPerUser = config.ReadCached(@"maxCount", DEFAULT_MAX_COUNT);
            TimeOut = config.ReadCached(@"timeOut", DEFAULT_TIMEOUT);
        }

        public void SendPacket(IServerPacket packet) {
            if(packet == null)
                throw new ArgumentNullException(nameof(packet));

            lock(Sync)
                foreach(Session session in Sessions)
                    session.SendPacket(packet);
        }

        public void SendPacket(Session session, IServerPacket packet) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            if(packet == null)
                throw new ArgumentNullException(nameof(packet));

            lock(Sync) {
                // this is here because i might add an ISession or some shit
                session.SendPacket(packet);
            }
        }

        public bool HasTimedOut(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            int timeOut = TimeOut;
            if(timeOut < 1) // avoid idiocy
                timeOut = DEFAULT_TIMEOUT;
            return session.GetIdleTime().TotalSeconds >= timeOut;
        }

        public ISession GetSession(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            lock(Sync) {
                if(session is ILocalSession s && LocalSessions.Contains(s))
                    return s;
                if(Sessions.Contains(session))
                    return session;
                return Sessions.FirstOrDefault(s => s.Equals(session));
            }
        }

        public ILocalSession GetLocalSession(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));
            lock(Sync) {
                if(session is ILocalSession s && Sessions.Contains(s))
                    return s;
                return LocalSessions.FirstOrDefault(s => s.Equals(session));
            }
        }

        public ILocalSession GetLocalSession(IConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));

            lock(Sync)
                return LocalSessions.FirstOrDefault(s => s.HasConnection(conn));
        }

        public void GetSessions(IUser user, Action<IEnumerable<ISession>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(Sessions.Where(s => s.HasUser() && s.User.Equals(user)));
        }

        public void GetLocalSessions(IUser user, Action<IEnumerable<ILocalSession>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(LocalSessions.Where(s => s.HasUser() && s.User.Equals(user)));
        }

        // i wonder what i'll think about this after sleeping a night on it
        // perhaps stick active sessions with the master User implementation again transparently.
        // session startups should probably be events as well
        public void GetSessions(IEnumerable<IUser> users, Action<IEnumerable<ISession>> callback) {
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(Sessions.Where(s => s.HasUser() && users.Any(s.User.Equals)));
        }

        public void GetLocalSessions(IEnumerable<IUser> users, Action<IEnumerable<ILocalSession>> callback) {
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(LocalSessions.Where(s => s.HasUser() && users.Any(s.User.Equals)));
        }

        public void GetActiveSessions(Action<IEnumerable<ISession>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(Sessions.Where(s => s.HasUser() && !HasTimedOut(s)));
        }

        public void GetActiveLocalSessions(Action<IEnumerable<ILocalSession>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(LocalSessions.Where(s => s.HasUser() && !HasTimedOut(s)));
        }

        public void GetDeadLocalSessions(Action<IEnumerable<ILocalSession>> callback) {
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock(Sync)
                callback.Invoke(LocalSessions.Where(HasTimedOut));
        }

        public ILocalSession Create(IConnection conn, IUser user) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            lock(Sync) {
                Session sess = new Session(ServerId, conn, user);
                LocalSessions.Add(sess);
                Sessions.Add(sess);
                Dispatcher.DispatchEvent(this, new SessionCreatedEvent(sess));
                return sess;
            }
        }

        public void Destroy(ISession session) {
            if(session == null)
                throw new ArgumentNullException(nameof(session));

            lock(Sync) {
                ILocalSession ls = GetLocalSession(session);
                if(ls != null) {
                    LocalSessions.Remove(ls);
                }

                Dispatcher.DispatchEvent(this, new SessionDestroyEvent(session));
            }
        }

        public bool HasSessions(IUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            lock(Sync)
                return Sessions.Any(s => s.HasUser() && s.User.Equals(user));
        }

        public int GetSessionCount(IUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            lock(Sync)
                return Sessions.Count(s => s.HasUser() && s.User.Equals(user));
        }

        public int GetAvailableSessionCount(IUser user) {
            return MaxPerUser - GetSessionCount(user);
        }

        public IEnumerable<IPAddress> GetRemoteAddresses(IUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            IEnumerable<IPAddress> addrs = Enumerable.Empty<IPAddress>();

            GetActiveSessions(sessions => {
                addrs = sessions.Where(s => s.User.Equals(user))
                                .OrderByDescending(s => s.LastPing)
                                .Select(s => s.RemoteAddress)
                                .Distinct();
            });

            return addrs;
        }

        public void CheckTimeOut() {
            lock(Sync) {
                IEnumerable<ILocalSession> sessions = null;
                GetDeadLocalSessions(s => sessions = s.ToArray());
                if(sessions == null || !sessions.Any())
                    return;
                foreach(ILocalSession session in sessions)
                    Destroy(session);
            }
        }

        public void HandleEvent(object sender, IEvent evt) {
            //
        }
    }
}
