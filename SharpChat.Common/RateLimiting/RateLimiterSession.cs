using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;

namespace SharpChat.RateLimiting {
    public class RateLimiterSession {
        public IHasSessions User { get; }
        public IConnection Connection { get; }
        public Queue<DateTimeOffset> TimePoints { get; } = new Queue<DateTimeOffset>();
        public object Sync { get; } = new object();

        public RateLimiterSession(IHasSessions user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public RateLimiterSession(IConnection conn) {
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        public bool IsMatch(IHasSessions user)
            => user != null && User == user;
        public bool IsMatch(IConnection conn)
            => conn != null ? Connection == conn : User.HasConnection(conn);
    }
}
