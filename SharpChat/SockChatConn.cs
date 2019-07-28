using Fleck;
using System;

namespace SharpChat
{
    public class SockChatConn
    {
        public readonly IWebSocketConnection Websocket;

        public DateTimeOffset LastPing { get; set; } = DateTimeOffset.UtcNow;

        public SockChatConn(IWebSocketConnection ws)
        {
            Websocket = ws;
        }

        public void Send(string data)
        {
            if (!Websocket.IsAvailable)
                return;
            Websocket.Send(data);
        }

        public void BumpPing()
            => LastPing = DateTimeOffset.Now;

        public bool HasTimedOut
            => DateTimeOffset.Now - LastPing > TimeSpan.FromMinutes(5);

        public void Close()
        {
            Websocket.Close();
        }
    }
}
