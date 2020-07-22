using Fleck;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharpChat {
    public class ChatUserSession : IDisposable, IPacketTarget {
        public const int ID_LENGTH = 32;

#if DEBUG
        public static TimeSpan SessionTimeOut { get; } = TimeSpan.FromMinutes(1);
#else
        public static TimeSpan SessionTimeOut { get; } = TimeSpan.FromMinutes(5);
#endif

        public IWebSocketConnection Connection { get; }

        public string Id { get; private set; }
        public bool IsDisposed { get; private set; }
        public DateTimeOffset LastPing { get; set; } = DateTimeOffset.MinValue;
        public ChatUser User { get; set; }

        public string TargetName => @"@log";


        private IPAddress _RemoteAddress = null;

        public IPAddress RemoteAddress {
            get {
                if (_RemoteAddress == null) {
                    if ((Connection.ConnectionInfo.ClientIpAddress == @"127.0.0.1" || Connection.ConnectionInfo.ClientIpAddress == @"::1")
                        && Connection.ConnectionInfo.Headers.ContainsKey(@"X-Real-IP"))
                        _RemoteAddress = IPAddress.Parse(Connection.ConnectionInfo.Headers[@"X-Real-IP"]);
                    else
                        _RemoteAddress = IPAddress.Parse(Connection.ConnectionInfo.ClientIpAddress);
                }

                return _RemoteAddress;

            }
        }

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

        public void Dispose()
            => Dispose(true);

        ~ChatUserSession()
            => Dispose(false);

        private void Dispose(bool disposing) {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Connection.Close();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
