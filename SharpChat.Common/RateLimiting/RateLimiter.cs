using SharpChat.Configuration;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.RateLimiting {
    public class RateLimiter : IDisposable {
        public const int DEFAULT_BAN_DURATION = 30;
        public const int DEFAULT_RANK_EXCEPT = 0;
        public const int DEFAULT_BACKLOG_SIZE = 30;
        public const int DEFAULT_THRESHOLD = 10;
        public const int DEFAULT_WARN_WITHIN = 5;

        private CachedValue<int> BanDuration { get; }
        private CachedValue<int> RankException { get; }
        private CachedValue<int> BacklogSize { get; }
        private CachedValue<int> Threshold { get; }
        private CachedValue<int> WarnWithin { get; }

        private List<RateLimiterSession> Sessions { get; } = new List<RateLimiterSession>();
        private object Sync { get; } = new object();

        public RateLimiter(IConfig config) {
            BanDuration = config.ReadCached(@"banDuration", DEFAULT_BAN_DURATION);
            RankException = config.ReadCached(@"exceptRank", DEFAULT_RANK_EXCEPT);
            BacklogSize = config.ReadCached(@"backlog", DEFAULT_BAN_DURATION);
            Threshold = config.ReadCached(@"threshold", DEFAULT_RANK_EXCEPT);
            WarnWithin = config.ReadCached(@"warnWithin", DEFAULT_RANK_EXCEPT);
        }

        public RateLimitState Bump(IHasSessions user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(BanDuration == 0)
                return RateLimitState.None;
            return BumpInternal(GetSession(user));
        }

        public RateLimitState Bump(IWebSocketConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            if(BanDuration == 0)
                return RateLimitState.None;
            return BumpInternal(GetSession(conn));
        }

        private RateLimitState BumpInternal(RateLimiterSession sess) {
            lock(sess.Sync) {
                if(CheckException(sess))
                    return RateLimitState.None;
                if(sess.TimePoints.Count >= BacklogSize)
                    sess.TimePoints.Dequeue();
                sess.TimePoints.Enqueue(DateTimeOffset.Now);
                return GetStateInternal(sess);
            }
        }

        public RateLimitState GetState(IHasSessions user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(BanDuration == 0)
                return RateLimitState.None;
            return GetStateInternal(GetSession(user));
        }

        public RateLimitState GetState(IWebSocketConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            if(BanDuration == 0)
                return RateLimitState.None;
            return GetStateInternal(GetSession(conn));
        }

        public RateLimitState GetStateInternal(RateLimiterSession sess) {
            lock(sess.Sync) {
                if(CheckException(sess))
                    return RateLimitState.None;

                if(sess.TimePoints.Count >= BacklogSize) {
                    int threshold = Threshold;
                    bool hasUser = sess.User != null,
                        hitThreshold = (sess.TimePoints.Last() - sess.TimePoints.First()).TotalSeconds <= threshold;

                    if(hasUser) {
                        if(hitThreshold)
                            return RateLimitState.Kick;
                        if((sess.TimePoints.Last() - sess.TimePoints.Skip(WarnWithin).First()).TotalSeconds <= threshold)
                            return RateLimitState.Warning;
                    } else if(hitThreshold)
                        return RateLimitState.Disconnect;
                }
            }

            return RateLimitState.None;
        }

        private bool CheckException(RateLimiterSession sess) {
            int exceptRank = RankException;
            return exceptRank > 0 && sess.User is IUser user && user.Rank >= exceptRank;
        }

        private RateLimiterSession GetSession(IHasSessions user) {
            lock(Sync) {
                RateLimiterSession sess = Sessions.Find(s => s.IsMatch(user));
                if(sess == null)
                    Sessions.Add(sess = new RateLimiterSession(user));
                return sess;
            }
        }

        private RateLimiterSession GetSession(IWebSocketConnection conn) {
            lock(Sync) {
                RateLimiterSession sess = Sessions.Find(s => s.IsMatch(conn));
                if(sess == null)
                    Sessions.Add(sess = new RateLimiterSession(conn));
                return sess;
            }
        }

        public void Remove(IHasSessions user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            lock(Sync) {
                Sessions.RemoveAll(s => s.IsMatch(user));
            }
        }

        public void Remove(IWebSocketConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            lock(Sync) {
                Sessions.RemoveAll(s => s.IsMatch(conn));
            }
        }

        private bool IsDisposed;
        ~RateLimiter()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;

            BanDuration.Dispose();
            RankException.Dispose();
            BacklogSize.Dispose();
            Threshold.Dispose();
            WarnWithin.Dispose();
        }
    }
}
