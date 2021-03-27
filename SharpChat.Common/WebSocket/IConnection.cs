using SharpChat.Packets;
using System.Net;

namespace SharpChat.WebSocket {
    public interface IConnection {
        IPAddress RemoteAddress { get; }
        IPAddress OriginalRemoteAddress { get; }
        bool IsLocal { get; }
        bool IsAvailable { get; }

        void Send(object obj);
        void Send(IServerPacket obj);

        void Close();
    }
}
