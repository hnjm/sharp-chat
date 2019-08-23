using Fleck;
using System;

namespace SharpChat
{
    public class SockChatConn : IDisposable
    {
        public readonly IWebSocketConnection Websocket;
        
        public int Version { get; set; } = 1;
        public bool IsDisposed { get; private set; }
        public DateTimeOffset LastPing { get; set; } = DateTimeOffset.UtcNow;

        public string RemoteAddress => Websocket.RemoteAddress();

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

        public void Send(SockChatServerPacket inst, params object[] parts)
            => Send(parts.Pack(inst));

        public void SendLog(IChatMessage msg)
            => Send(SockChatServerPacket.ContextPopulate, Constants.CTX_MSG, msg);

        public void BumpPing()
            => LastPing = DateTimeOffset.Now;

        public bool HasTimedOut
            => DateTimeOffset.Now - LastPing > TimeSpan.FromMinutes(5);

        public void Dispose()
            => Dispose(true);

        ~SockChatConn()
            => Dispose(false);

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Websocket.Close();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
