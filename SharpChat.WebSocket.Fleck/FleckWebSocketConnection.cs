using System;
using System.Net;
using IFleckWebSocketConnection = Fleck.IWebSocketConnection;

namespace SharpChat.WebSocket.Fleck {
    public class FleckWebSocketConnection : IWebSocketConnection {
        public IPAddress RemoteAddress { get; init; }
        public IPAddress OriginalRemoteAddress { get; init; }

        public bool IsLocal => IPAddress.IsLoopback(OriginalRemoteAddress);
        public bool IsAvailable => Connection.IsAvailable;

        private IFleckWebSocketConnection Connection { get; init; }

        public FleckWebSocketConnection(IFleckWebSocketConnection conn) {
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
            OriginalRemoteAddress = IPAddress.Parse(Connection.ConnectionInfo.ClientIpAddress);
            RemoteAddress = IsLocal && Connection.ConnectionInfo.Headers.ContainsKey(@"X-Real-IP")
                ? IPAddress.Parse(Connection.ConnectionInfo.Headers[@"X-Real-IP"])
                : OriginalRemoteAddress;
        }

        public void Send(object obj)
            => Connection.Send(obj.ToString());

        private bool IsDisposed;
        ~FleckWebSocketConnection()
            => Dispose(false);
        public void Dispose()
            => Dispose(true);
        private void Dispose(bool disposing) {
            if(IsDisposed)
                return;
            IsDisposed = true;

            Connection.Close();

            if(disposing)
                GC.SuppressFinalize(this);
        }
    }
}
