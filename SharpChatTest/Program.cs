using PureWebSockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpChatTest {
    class Program {
        static int UserId = 10000;

        static void Main() {
            PureWebSocket[] socks = new PureWebSocket[500];

            for (int i = 0; i < socks.Length; i++) {
                socks[i] = new PureWebSocket(@"ws://127.0.0.1:6770/", new PureWebSocketOptions { });
                socks[i].OnOpened += Program_OnOpened;
                socks[i].OnClosed += Program_OnClosed;
                socks[i].OnSendFailed += Program_OnSendFailed;
                socks[i].OnStateChanged += Program_OnStateChanged;
                socks[i].OnError += Program_OnError;
                socks[i].OnFatality += Program_OnFatality;
                socks[i].OnMessage += Program_OnMessage;
                socks[i].OnData += Program_OnData;
            }

            Parallel.ForEach(socks, sock => sock.Connect());

            string msg;
            while ((msg = Console.ReadLine()) != @";q") {
                string pack = $"2\t0\t{msg}";
                Parallel.ForEach(socks, sock => Send(sock, pack));
            }

            while (!Parallel.ForEach(socks, sock => sock.Dispose()).IsCompleted) ;

            Console.ReadLine();
        }

        public static void Send(PureWebSocket ws, string message) {
            Console.WriteLine($@"{ws.InstanceName} > {message}");
            ws.Send(message);
        }

        private static void Program_OnData(object sender, byte[] data) {
            PureWebSocket ws = sender as PureWebSocket;
            Console.WriteLine($@"{ws.InstanceName} < " + System.Text.Encoding.ASCII.GetString(data));
        }

        private static void Program_OnMessage(object sender, string message) {
            PureWebSocket ws = sender as PureWebSocket;
            Console.WriteLine($@"{ws.InstanceName} < {message}");
        }

        private static void Program_OnFatality(object sender, string reason) {
            PureWebSocket ws = sender as PureWebSocket;
            Console.WriteLine($@"{ws.InstanceName}: Fatality - {reason}");
        }

        private static void Program_OnError(object sender, Exception ex) {
            PureWebSocket ws = sender as PureWebSocket;
            Console.WriteLine($@"{ws.InstanceName}: {ex.GetType().Name} - {ex.Message}");
        }

        private static void Program_OnStateChanged(object sender, System.Net.WebSockets.WebSocketState newState, System.Net.WebSockets.WebSocketState prevState) {
            PureWebSocket ws = sender as PureWebSocket;
            Console.WriteLine($@"{ws.InstanceName}: {prevState} -> {newState}");
        }

        private static void Program_OnSendFailed(object sender, string data, Exception ex) {
            PureWebSocket ws = sender as PureWebSocket;
            Console.WriteLine($@"{ws.InstanceName}: Failed to send ""{data}""");
        }

        private static void Program_OnClosed(object sender, System.Net.WebSockets.WebSocketCloseStatus reason) {
            PureWebSocket ws = sender as PureWebSocket;
            Console.WriteLine($@"{ws.InstanceName}: Closed {reason}");
        }

        private static void Program_OnOpened(object sender) {
            PureWebSocket ws = sender as PureWebSocket;
            Console.WriteLine($@"{ws.InstanceName}: Opened");

            Send(ws, $"1\t{Interlocked.Increment(ref UserId)}\tmewow");
        }
    }
}
