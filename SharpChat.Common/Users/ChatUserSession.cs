using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharpChat.Users {
    public class ChatUserSession : IDisposable, IPacketTarget {
        public const int ID_LENGTH = 32;

#if DEBUG
        public static TimeSpan SessionTimeOut { get; } = TimeSpan.FromMinutes(1);
#else
        public static TimeSpan SessionTimeOut { get; } = TimeSpan.FromMinutes(5);
#endif

        public IWebSocketConnection Connection { get; }

        public string Id { get; private set; }
        public DateTimeOffset LastPing { get; set; } = DateTimeOffset.MinValue;
        public ChatUser User { get; set; }

        public string TargetName => @"@log";

        public bool HasUser
            => User != null;

        public IPAddress RemoteAddress
            => Connection.RemoteAddress;

        public ChatUserSession(IWebSocketConnection ws) {
            Connection = ws;
            Id = GenerateId();
        }

        private static string GenerateId() {
            byte[] buffer = new byte[ID_LENGTH];
            RNG.NextBytes(buffer);
            return buffer.GetIdString();
        }

        public void Send(IServerPacket packet) {
            if (!Connection.IsAvailable)
                return;

            IEnumerable<string> data = packet.Pack();

            if (data != null)
                foreach (string line in data)
                    if (!string.IsNullOrWhiteSpace(line))
                        Connection.Send(line);
        }

        public void BumpPing()
            => LastPing = DateTimeOffset.Now;

        public bool HasTimedOut
            => DateTimeOffset.Now - LastPing > SessionTimeOut;

        public bool IsAlive
            => !HasTimedOut && !IsDisposed;

        private bool IsDisposed;

        ~ChatUserSession()
            => DoDispose();

        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }

        private void DoDispose() {
            if (IsDisposed)
                return;
            IsDisposed = true;
            Connection.Dispose();
        }
    }
}
