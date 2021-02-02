using Fleck;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IFleckWebSocketConnection = Fleck.IWebSocketConnection;
using IFleckWebSocketServer = Fleck.IWebSocketServer;

// Near direct reimplementation of Fleck's WebSocketServer with address reusing
// Fleck's Socket wrapper doesn't provide any way to do this with the normally provided APIs
// https://github.com/statianzo/Fleck/blob/1.1.0/src/Fleck/WebSocketServer.cs

namespace SharpChat.WebSocket.Fleck {
    public class FleckServer : IServer {
        public event Action<IConnection> OnOpen;
        public event Action<IConnection> OnClose;
        public event Action<IConnection, Exception> OnError;
        public event Action<IConnection, string> OnMessage;

        private FleckWebSocketServer Server { get; set; }
        private IPEndPoint EndPoint { get; }

        public FleckServer(IPEndPoint endPoint) {
            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
        }

        public void Start() {
            if(Server != null)
                throw new Exception(@"A server has already been started.");
            Server = new FleckWebSocketServer(EndPoint);
            Server.Start(rawConn => {
                FleckConnection conn = new FleckConnection(rawConn);
                rawConn.OnOpen += () => OnOpen?.Invoke(conn);
                rawConn.OnClose += () => { OnClose?.Invoke(conn); conn.Dispose(); };
                rawConn.OnError += ex => OnError?.Invoke(conn, ex);
                rawConn.OnMessage += msg => OnMessage?.Invoke(conn, msg);
            });
        }

        private bool IsDisposed;

        ~FleckServer()
            => DoDispose();

        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }

        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;
            Server?.Dispose();
        }

        private class FleckWebSocketServer : IFleckWebSocketServer {
            private readonly string _scheme;
            private readonly IPAddress _locationIP;
            private Action<IFleckWebSocketConnection> _config;

            public EndPoint EndPoint { get; }

            public FleckWebSocketServer(IPEndPoint endPoint, bool secure = false) {
                EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
                _scheme = secure ? @"wss" : @"ws";

                Port = endPoint.Port;
                _locationIP = endPoint.Address;
                Location = string.Format(@"{0}://{1}:{2}/", _scheme, _locationIP, Port);

                SupportDualStack = true;

                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.IP);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

                if(SupportDualStack && Type.GetType(@"Mono.Runtime") == null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                }

                ListenerSocket = new SocketWrapper(socket);
                SupportedSubProtocols = Array.Empty<string>();
            }

            public ISocket ListenerSocket { get; set; }
            public string Location { get; private set; }
            public bool SupportDualStack { get; }
            public int Port { get; private set; }
            public X509Certificate2 Certificate { get; set; }
            public SslProtocols EnabledSslProtocols { get; set; }
            public IEnumerable<string> SupportedSubProtocols { get; set; }
            public bool RestartAfterListenError { get; set; }

            public bool IsSecure {
                get { return _scheme == "wss" && Certificate != null; }
            }

            public void Dispose() {
                ListenerSocket.Dispose();
                GC.SuppressFinalize(this);
            }

            public void Start(Action<IFleckWebSocketConnection> config) {
                ListenerSocket.Bind(EndPoint);
                ListenerSocket.Listen(100);
                if(ListenerSocket.LocalEndPoint is IPEndPoint ipep)
                    Port = ipep.Port;
                FleckLog.Info(string.Format("Server started at {0} (actual port {1})", Location, Port));
                if(_scheme == "wss") {
                    if(Certificate == null) {
                        FleckLog.Error("Scheme cannot be 'wss' without a Certificate");
                        return;
                    }

                    if(EnabledSslProtocols == SslProtocols.None) {
                        EnabledSslProtocols = SslProtocols.Tls;
                        FleckLog.Debug("Using default TLS 1.0 security protocol.");
                    }
                }
                ListenForClients();
                _config = config;
            }

            private void ListenForClients() {
                ListenerSocket.Accept(OnClientConnect, e => {
                    FleckLog.Error("Listener socket is closed", e);
                    if(RestartAfterListenError) {
                        FleckLog.Info("Listener socket restarting");
                        try {
                            ListenerSocket.Dispose();
                            Socket socket = new Socket(_locationIP.AddressFamily, SocketType.Stream, ProtocolType.IP);
                            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                            ListenerSocket = new SocketWrapper(socket);
                            Start(_config);
                            FleckLog.Info("Listener socket restarted");
                        } catch(Exception ex) {
                            FleckLog.Error("Listener could not be restarted", ex);
                        }
                    }
                });
            }

            private void OnClientConnect(ISocket clientSocket) {
                if(clientSocket == null)
                    return; // socket closed

                FleckLog.Debug(string.Format("Client connected from {0}:{1}", clientSocket.RemoteIpAddress, clientSocket.RemotePort.ToString()));
                ListenForClients();

                WebSocketConnection connection = null;

                connection = new WebSocketConnection(
                    clientSocket,
                    _config,
                    bytes => RequestParser.Parse(bytes, _scheme),
                    r => {
                        try {
                            return HandlerFactory.BuildHandler(
                                r, s => connection.OnMessage(s), connection.Close, b => connection.OnBinary(b),
                                b => connection.OnPing(b), b => connection.OnPong(b)
                            );
                        } catch(WebSocketException) {
                            const string responseMsg = "HTTP/1.1 200 OK\r\n"
                                                     + "Date: {0}\r\n"
                                                     + "Server: SharpChat\r\n"
                                                     + "Content-Length: {1}\r\n"
                                                     + "Content-Type: text/html; charset=utf-8\r\n"
                                                     + "Connection: close\r\n"
                                                     + "\r\n"
                                                     + "{2}";
                            string responseBody = File.Exists(@"http-motd.txt") ? File.ReadAllText(@"http-motd.txt") : @"SharpChat";

                            clientSocket.Stream.Write(Encoding.UTF8.GetBytes(string.Format(
                                responseMsg, DateTimeOffset.Now.ToString(@"r"), Encoding.UTF8.GetByteCount(responseBody), responseBody
                            )));
                            clientSocket.Close();
                            return null;
                        }
                    },
                    s => SubProtocolNegotiator.Negotiate(SupportedSubProtocols, s));

                if(IsSecure) {
                    FleckLog.Debug("Authenticating Secure Connection");
                    clientSocket
                        .Authenticate(Certificate,
                                      EnabledSslProtocols,
                                      connection.StartReceiving,
                                      e => FleckLog.Warn("Failed to Authenticate", e));
                } else {
                    connection.StartReceiving();
                }
            }
        }
    }
}
