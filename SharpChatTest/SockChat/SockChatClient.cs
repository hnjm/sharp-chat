using PureWebSockets;
using System;
using System.Net.WebSockets;

namespace SharpChatTest.SockChat {
    public class SockChatClient : IDisposable {
        public string SpoofedIP { get; }
        private PureWebSocket WebSocket { get; }

        public int UserId { get; }
        public bool IsConnected { get; private set; }

        public SockChatClient(ushort port, int userId) {
            UserId = userId;
            Random rng = new Random(userId);
            SpoofedIP = rng.Next(1, 254).ToString() + '.' + rng.Next(1, 254) + '.' + rng.Next(1, 254) + '.' + rng.Next(1, 254);
            WebSocket = new PureWebSocket($@"ws://127.0.0.1:{port}/", new PureWebSocketOptions {
                Headers = new Tuple<string, string>[] {
                    new Tuple<string, string>(@"X-Real-IP", SpoofedIP),
                },
            });
            WebSocket.OnOpened += WebSocket_OnOpened;
            WebSocket.OnClosed += WebSocket_OnClosed;
            WebSocket.OnError += WebSocket_OnError;
            WebSocket.OnMessage += WebSocket_OnMessage;
            WebSocket.Connect();
        }

        private void WebSocket_OnOpened(object sender) {
            IsConnected = true;
        }

        private void WebSocket_OnClosed(object sender, WebSocketCloseStatus reason) {
            IsConnected = false;
        }

        private void WebSocket_OnError(object sender, Exception ex) {
            //
        }

        private void WebSocket_OnMessage(object sender, string message) {
            //
        }

        private bool IsDisposed;

        ~SockChatClient()
            => DoDispose();

        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }

        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;

            WebSocket.Dispose();
        }
    }
}
