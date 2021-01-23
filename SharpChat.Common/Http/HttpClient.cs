using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SharpChat.Http {
    public static class HttpClient {
        public const string USER_AGENT = @"SharpChat/1.0";

        private static readonly object Lock = new object();
        private static readonly List<HttpClientConnection> Connections = new List<HttpClientConnection>();

        private static HttpClientConnection GetConnection(IPAddress addr) {
            lock(Lock)
                return Connections.FirstOrDefault(x => x.Address == addr);
        }

        private static HttpClientConnection NewConnection(IPAddress addr) {
            HttpClientConnection conn;
            lock(Lock) {
                RemoveConnection(addr);

                Socket sock = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {
                    NoDelay = true
                };

                conn = new HttpClientConnection(addr, sock);
                Connections.Add(conn);
            }
            return conn;
        }

        private static void RemoveConnection(IPAddress addr) {
            lock(Lock) {
                HttpClientConnection conn = Connections.FirstOrDefault(x => x.Address == addr);
                if(conn != null)
                    RemoveConnection(conn);
            }
        }

        private static void RemoveConnection(HttpClientConnection conn) {
            conn.Dispose();
            lock(Lock)
                Connections.Remove(conn);
        }

        public static void Cleanup() {
            lock(Lock) {
                foreach(HttpClientConnection conn in Connections)
                    conn.Dispose();
                Connections.Clear();
            }
        }

        // TODO
        //  - Threading
        //  - Proper exceptions
        //  - More
        public static HttpResponseMessage Send(HttpRequestMessage request) {
            if(!request.HasHeader(HttpUserAgentHeader.NAME))
                request.SetHeader(HttpUserAgentHeader.NAME, USER_AGENT);

            IPAddress[] addrs = Dns.GetHostAddresses(request.Host);

            if(!addrs.Any())
                throw new Exception(@"No addresses found for this host.");

            HttpResponseMessage response = null;
            Exception exception = null;
            HttpClientConnection conn = null;

            foreach(IPAddress addr in addrs) {
                exception = null;
                conn = Connections.FirstOrDefault(x => x.Address == addr);

                try {
                    request.WriteTo(conn.Stream);
                } catch(IOException) {
                    conn = NewConnection(addr);
                    try {
                        request.WriteTo(conn.Stream);
                        break;
                    } catch(IOException ex) {
                        exception = ex;
                        continue;
                    }
                }
            }

            if(conn == null)
                throw new Exception(@"No connection.");

            response = HttpResponseMessage.ReadFrom(conn.Stream);

            if(exception != null)
                throw exception;
            if(response == null)
                throw new Exception(@"Request failed.");
            return response;
        }
    }
}
