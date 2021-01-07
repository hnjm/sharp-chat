using PureWebSockets;
using SharpChat;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace SharpChatTest.SockChat {
    public class SockChatClient : IDisposable {
        public string SpoofedIP { get; }
        private PureWebSocket WebSocket { get; }

        public int UserId { get; }
        public bool IsConnected { get; private set; }

        public List<IEnumerable<string>> PingLog { get; } = new List<IEnumerable<string>>();
        public List<IEnumerable<string>> PacketLog { get; } = new List<IEnumerable<string>>();

        public DateTimeOffset LastPing { get; private set; } = DateTimeOffset.MinValue;

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
            Logger.ClientWriteLine(@$"[{UserId:00000}] Connected");
        }

        private void WebSocket_OnClosed(object sender, WebSocketCloseStatus reason) {
            IsConnected = false;
            Logger.ClientWriteLine(@$"[{UserId:00000}] Disconnected: {reason}");
        }

        private void WebSocket_OnError(object sender, Exception ex) {
            Logger.ErrorWriteLine(@$"[{UserId:00000}] {ex}");
        }

        private void WebSocket_OnMessage(object sender, string message) {
            string[] parts = message.Split('\t');
            (parts[0] == @"0" ? PingLog : PacketLog).Add(parts);
            Logger.ClientWriteLine($@"[{UserId:00000}] Received packet {parts[0]}");
        }

        public void ClearPacketLog() {
            PacketLog.Clear();
        }

        public void SendPing() {
            LastPing = DateTimeOffset.Now;
            WebSocket.Send($"{(int)SockChatClientPacket.Ping}\t{UserId}\t{LastPing.ToUnixTimeSeconds()}");
        }

        public void SendLogin(string argument = @"soapsoapsoap") {
            WebSocket.Send($"{(int)SockChatClientPacket.Authenticate}\t{UserId}\t{argument}");
        }

        public void SendMessage(string message, string channel = @"Lounge") {
            WebSocket.Send($"{(int)SockChatClientPacket.MessageSend}\t{UserId}\t{message}\t{channel}");
        }

        public void SendTyping(string channel = @"Lounge") {
            WebSocket.Send($"{(int)SockChatClientPacket.Typing}\t{UserId}\t{channel}");
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
