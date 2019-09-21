using Fleck;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharpChat
{
    public class ChatUserConnection : IDisposable
    {
        public readonly IWebSocketConnection Websocket;
        
        public int Version { get; set; } = 1;
        public bool IsDisposed { get; private set; }
        public DateTimeOffset LastPing { get; set; } = DateTimeOffset.MinValue;

        private IPAddress _RemoteAddress = null;

        public IPAddress RemoteAddress
        {
            get
            {
                if(_RemoteAddress == null)
                {
                    if ((Websocket.ConnectionInfo.ClientIpAddress == @"127.0.0.1" || Websocket.ConnectionInfo.ClientIpAddress == @"::1")
                        && Websocket.ConnectionInfo.Headers.ContainsKey(@"X-Real-IP"))
                        _RemoteAddress = IPAddress.Parse(Websocket.ConnectionInfo.Headers[@"X-Real-IP"]);
                    else
                        _RemoteAddress = IPAddress.Parse(Websocket.ConnectionInfo.ClientIpAddress);
                }

                return _RemoteAddress;

            }
        }

        public ChatUserConnection(IWebSocketConnection ws)
        {
            Websocket = ws;
        }

        [Obsolete(@"Use Send(IServerPacket, int)")]
        public void Send(string data)
        {
            if (!Websocket.IsAvailable)
                return;
            Websocket.Send(data);
        }

        public void Send(IServerPacket packet)
        {
            if (!Websocket.IsAvailable)
                return;

            IEnumerable<string> data = packet.Pack(Version);

            if(data != null)
                foreach(string line in data)
                    if(!string.IsNullOrWhiteSpace(line))
                        Websocket.Send(line);
        }

        public void BumpPing()
            => LastPing = DateTimeOffset.Now;

        public bool HasTimedOut
            => DateTimeOffset.Now - LastPing > TimeSpan.FromMinutes(5);

        public void Dispose()
            => Dispose(true);

        ~ChatUserConnection()
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
