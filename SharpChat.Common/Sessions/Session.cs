using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharpChat.Sessions {
    public class Session : IDisposable, IPacketTarget {
        public const int ID_LENGTH = 32;

        public IWebSocketConnection Connection { get; }

        public string Id { get; private set; }
        public DateTimeOffset LastPing { get; set; }
        public ChatUser User { get; set; }

        public TimeSpan IdleTime => DateTimeOffset.Now - LastPing;

        public string TargetName => @"@log";

        public bool HasUser
            => User != null;

        public IPAddress RemoteAddress
            => Connection.RemoteAddress;

        public Session(IWebSocketConnection ws) {
            Connection = ws;
            Id = RNG.NextString(ID_LENGTH);
            BumpPing();
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

        private bool IsDisposed;
        ~Session()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if (IsDisposed)
                return;
            IsDisposed = true;
            if(HasUser)
                User.RemoveSession(this);
            Connection.Dispose();
            LastPing = DateTimeOffset.MinValue;
        }
    }
}
