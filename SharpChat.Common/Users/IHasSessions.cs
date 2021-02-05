using SharpChat.Sessions;
using SharpChat.WebSocket;

namespace SharpChat.Users {
    public interface IHasSessions {
        /// <summary>
        /// Register a session
        /// </summary>
        void AddSession(Session session);

        /// <summary>
        /// Unregister a session
        /// </summary>
        void RemoveSession(Session session);

        /// <summary>
        /// Check if a session is registered here
        /// </summary>
        bool HasSession(Session session);

        /// <summary>
        /// Check if a connection is associated with a registered session here
        /// </summary>
        bool HasConnection(IConnection connection);

        /// <summary>
        /// Checks if any registered session has this capability
        /// </summary>
        bool HasCapability(ClientCapabilities capability);
    }
}
