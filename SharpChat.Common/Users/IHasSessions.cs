using SharpChat.Sessions;
using SharpChat.WebSocket;

namespace SharpChat.Users {
    public interface IHasSessions {
        void AddSession(Session session);
        void RemoveSession(Session session);
        bool HasSession(Session session);
        bool HasConnection(IWebSocketConnection connection);
    }
}
