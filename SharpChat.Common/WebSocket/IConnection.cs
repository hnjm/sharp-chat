using System;
using System.Net;

namespace SharpChat.WebSocket {
    public interface IConnection : IDisposable {
        IPAddress RemoteAddress { get; }
        IPAddress OriginalRemoteAddress { get; }
        bool IsLocal { get; }
        bool IsAvailable { get; }

        void Send(object obj);
    }
}
