﻿using Fleck;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

// Near direct reimplementation of Fleck's WebSocketServer with address reusing
// Fleck's Socket wrapper doesn't provide any way to do this with the normally provided APIs
// https://github.com/statianzo/Fleck/blob/1.1.0/src/Fleck/WebSocketServer.cs

namespace SharpChat {
    public class SharpChatWebSocketServer : IWebSocketServer {

        private readonly string _scheme;
        private readonly IPAddress _locationIP;
        private Action<IWebSocketConnection> _config;

        public SharpChatWebSocketServer(string location, bool supportDualStack = true) {
            Uri uri = new Uri(location);

            Port = uri.Port;
            Location = location;
            SupportDualStack = supportDualStack;

            _locationIP = ParseIPAddress(uri);
            _scheme = uri.Scheme;
            Socket socket = new Socket(_locationIP.AddressFamily, SocketType.Stream, ProtocolType.IP);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            if (SupportDualStack && Type.GetType(@"Mono.Runtime") == null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            }

            ListenerSocket = new SocketWrapper(socket);
            SupportedSubProtocols = new string[0];
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
        }

        private IPAddress ParseIPAddress(Uri uri) {
            string ipStr = uri.Host;

            if (ipStr == "0.0.0.0") {
                return IPAddress.Any;
            } else if (ipStr == "[0000:0000:0000:0000:0000:0000:0000:0000]") {
                return IPAddress.IPv6Any;
            } else {
                try {
                    return IPAddress.Parse(ipStr);
                } catch (Exception ex) {
                    throw new FormatException("Failed to parse the IP address part of the location. Please make sure you specify a valid IP address. Use 0.0.0.0 or [::] to listen on all interfaces.", ex);
                }
            }
        }

        public void Start(Action<IWebSocketConnection> config) {
            IPEndPoint ipLocal = new IPEndPoint(_locationIP, Port);
            ListenerSocket.Bind(ipLocal);
            ListenerSocket.Listen(100);
            Port = ((IPEndPoint)ListenerSocket.LocalEndPoint).Port;
            FleckLog.Info(string.Format("Server started at {0} (actual port {1})", Location, Port));
            if (_scheme == "wss") {
                if (Certificate == null) {
                    FleckLog.Error("Scheme cannot be 'wss' without a Certificate");
                    return;
                }

                if (EnabledSslProtocols == SslProtocols.None) {
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
                if (RestartAfterListenError) {
                    FleckLog.Info("Listener socket restarting");
                    try {
                        ListenerSocket.Dispose();
                        Socket socket = new Socket(_locationIP.AddressFamily, SocketType.Stream, ProtocolType.IP);
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                        ListenerSocket = new SocketWrapper(socket);
                        Start(_config);
                        FleckLog.Info("Listener socket restarted");
                    } catch (Exception ex) {
                        FleckLog.Error("Listener could not be restarted", ex);
                    }
                }
            });
        }

        private void OnClientConnect(ISocket clientSocket) {
            if (clientSocket == null) return; // socket closed

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

            if (IsSecure) {
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
