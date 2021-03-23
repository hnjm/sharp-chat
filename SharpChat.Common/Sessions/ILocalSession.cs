using SharpChat.Packets;
using SharpChat.WebSocket;
using System;

namespace SharpChat.Sessions {
    public interface ILocalSession : ISession, IDisposable {
        bool HasConnection(IConnection conn);
        void Suspend();
        void Resume(IConnection conn);

        void SendPacket(IServerPacket packet);
    }
}
