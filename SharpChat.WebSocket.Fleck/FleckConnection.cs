using Fleck;
using SharpChat.Packets;
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

        public void Send(IServerPacket packet)
            => Connection.Send(packet.Pack());

        public void Close()
            => Connection.Close();

        public override string ToString()
            => $@"C#{RemoteAddress}:{Port}";
    }
}
