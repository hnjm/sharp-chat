using SharpChat.Packets;
using System;
using System.Net;

namespace SharpChat.WebSocket {
    public interface IConnection : IDisposable {
        IPAddress RemoteAddress { get; }
        IPAddress OriginalRemoteAddress { get; }
        bool IsLocal { get; }
        bool IsAvailable { get; }
        string Id { get; }

        void Send(object obj);
        void Send(IServerPacket obj);
    }
}
