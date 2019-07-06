using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SquidChat
{
    public class SockChatUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Colour { get; set; }
        public int Hierarchy { get; set; }

        public readonly List<SockChatConn> Connections = new List<SockChatConn>();

        public SockChatUser()
        {
            //
        }

        public SockChatUser(FlashiiAuthResult auth)
        {
            UserId = auth.UserId;
            Username = auth.Username;
            Colour = auth.Colour;
            Hierarchy = auth.Hierarchy;
        }

        public void Send(string data)
            => Connections.ForEach(c => c.Send(data));

        public void AddConnection(SockChatConn conn)
            => Connections.Add(conn);

        public void AddConnection(IWebSocketConnection conn)
            => Connections.Add(new SockChatConn(conn));

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
            sb.Append('\t');
            sb.Append(Username);
            sb.Append('\t');
            sb.Append(Colour);
            sb.Append('\t');
            sb.Append(@"1 0 0 0 0"); // RANK MOD LOG NICK CHAN (0/1/2)

            return sb.ToString();
        }
    }
}
