using Fleck;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SquidChat
{
    public enum SockChatUserChannel
    {
        No = 0,
        OnlyTemporary = 1,
        Yes = 2,
    }

    public class SockChatUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Colour { get; set; }
        public int Hierarchy { get; set; }
        public string Nickname { get; set; }

        public bool IsModerator { get; private set; } = false;
        public bool CanChangeNick { get; private set; } = false;
        public SockChatUserChannel CanCreateChannels { get; private set; } = SockChatUserChannel.No;

        public readonly List<SockChatConn> Connections = new List<SockChatConn>();

        public string DisplayName
            => !string.IsNullOrEmpty(Nickname) ? '~' + Nickname : Username;

        public IEnumerable<string> RemoteAddresses
            => Connections.Select(c => c.Websocket.ConnectionInfo.ClientIpAddress);

        public SockChatUser()
        {
        }

        public SockChatUser(FlashiiAuth auth)
        {
            UserId = auth.UserId;
            Username = auth.Username;
            Colour = auth.Colour;
            Hierarchy = auth.Hierarchy;
            IsModerator = auth.IsModerator;
            CanChangeNick = auth.CanChangeNick;
            CanCreateChannels = auth.CanCreateChannels;
        }

        public void Send(string data)
            => Connections.ForEach(c => c.Send(data));

        public void Send(SockChatClientMessage inst, params string[] parts)
            => Send(parts.Pack(inst));

        public void AddConnection(SockChatConn conn)
            => Connections.Add(conn);

        public void AddConnection(IWebSocketConnection conn)
            => Connections.Add(new SockChatConn(conn));

        public void RemoveConnection(SockChatConn conn)
           => Connections.Remove(conn);

        public void RemoveConnection(IWebSocketConnection conn)
            => Connections.Remove(Connections.FirstOrDefault(x => x.Websocket == conn));

        public bool HasConnection(SockChatConn conn)
            => Connections.Contains(conn);

        public bool HasConnection(IWebSocketConnection ws)
            => Connections.Any(x => x.Websocket == ws);

        public SockChatConn GetConnection(IWebSocketConnection ws)
            => Connections.FirstOrDefault(x => x.Websocket == ws);

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(UserId);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Username);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Colour);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Hierarchy);
            sb.Append(' ');
            sb.Append(IsModerator.AsChar());
            sb.Append(@" 0 ");
            sb.Append(CanChangeNick.AsChar());
            sb.Append(' ');
            sb.Append((int)CanCreateChannels);

            return sb.ToString();
        }
    }
}
