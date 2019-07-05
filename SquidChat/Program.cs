using Fleck;
using System;

namespace SquidChat
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("SquidChat - Multi-user (PHP) Sock Chat");

            WebSocketServer srv = new WebSocketServer("ws://0.0.0.0:6770");
            srv.Start(s =>
            {
                s.OnOpen = () => OnOpen(s);
                s.OnClose = () => OnClose(s);
                s.OnError = err => OnError(s, err);
                s.OnMessage = msg => OnMessage(s, msg);
            });
            Console.ReadLine();
        }

        private static void OnOpen(IWebSocketConnection conn)
        {
            Console.WriteLine($@"[{conn.ConnectionInfo.ClientIpAddress}] Open");
        }

        private static void OnClose(IWebSocketConnection conn)
        {
            Console.WriteLine($@"[{conn.ConnectionInfo.ClientIpAddress}] Close");
        }

        private static void OnError(IWebSocketConnection conn, Exception ex)
        {
            Console.WriteLine($@"[{conn.ConnectionInfo.ClientIpAddress}] Err {ex}");
        }

        private static void OnMessage(IWebSocketConnection conn, string msg)
        {
            Console.WriteLine($@"[{conn.ConnectionInfo.ClientIpAddress}] {msg}");

            string[] args = msg.Split('\t');

            if (args.Length < 1 || !Enum.TryParse(args[0], out SockChatServerMessage opCode))
                return;

            switch(opCode)
            {
                case SockChatServerMessage.Ping:
                    break;

                case SockChatServerMessage.Authenticate:
                    break;

                case SockChatServerMessage.MessageSend:
                    break;
            }
        }
    }
}
