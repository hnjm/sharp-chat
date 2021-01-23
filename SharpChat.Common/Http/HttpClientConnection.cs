using System;
using System.Net;
using System.Net.Sockets;

namespace SharpChat.Http {
    public class HttpClientConnection : IDisposable {
        public IPAddress Address { get; }
        public NetworkStream Stream { get; }

        public HttpClientConnection(IPAddress address, Socket socket) : this(
            address, new NetworkStream(socket ?? throw new ArgumentNullException(nameof(socket)), true)
        ) {}

        public HttpClientConnection(IPAddress address, NetworkStream stream) {
            if(address.AddressFamily != AddressFamily.InterNetwork
                && address.AddressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException(@"Address must be an IPv4 or IPv6 address.", nameof(address));
            Address = address;
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        private bool IsDisposed;
        ~HttpClientConnection()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;
            Stream.Dispose();
        }
    }
}
