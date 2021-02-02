using System;

namespace SharpChat.WebSocket {
    public interface IServer : IDisposable {
        event Action<IConnection> OnOpen;
        event Action<IConnection> OnClose;
        event Action<IConnection, Exception> OnError;
        event Action<IConnection, string> OnMessage;

        void Start();
    }
}
