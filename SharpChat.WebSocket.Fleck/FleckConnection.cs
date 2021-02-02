using Fleck;
using System;
using System.Net;

namespace SharpChat.WebSocket.Fleck {
    public class FleckConnection : IConnection {
        public IPAddress RemoteAddress { get; init; }
        public IPAddress OriginalRemoteAddress { get; init; }

        public ushort Port { get; }

        public bool IsLocal => IPAddress.IsLoopback(OriginalRemoteAddress);
        public bool IsAvailable => Connection.IsAvailable;

        private IWebSocketConnection Connection { get; init; }

        public FleckConnection(IWebSocketConnection conn) {
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
            Port = (ushort)Connection.ConnectionInfo.ClientPort;
            OriginalRemoteAddress = IPAddress.Parse(Connection.ConnectionInfo.ClientIpAddress);
            RemoteAddress = IsLocal && Connection.ConnectionInfo.Headers.ContainsKey(@"X-Real-IP")
                ? IPAddress.Parse(Connection.ConnectionInfo.Headers[@"X-Real-IP"])
                : OriginalRemoteAddress;
        }

        public void Send(object obj)
            => Connection.Send(obj.ToString());

        public override string ToString() {
            return string.Format(@"{0}:{1}", RemoteAddress, Port);
        }

        private bool IsDisposed;
        ~FleckConnection()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;
            Connection.Close();
        }
    }
}
