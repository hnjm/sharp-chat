using System;

namespace SharpChat.WebSocket {
    public interface IWebSocketServer : IDisposable {
        event Action<IWebSocketConnection> OnOpen;
        event Action<IWebSocketConnection> OnClose;
        event Action<IWebSocketConnection, Exception> OnError;
        event Action<IWebSocketConnection, string> OnMessage;

        void Start();
    }
}
