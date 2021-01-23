using System;
using System.IO;
using System.Linq;
using System.Net;

namespace SharpChat.Http {
    public static class HttpClient {
        public const string USER_AGENT = @"SharpChat/1.0";

        // TODO
        //  - Threading
        //  - Proper exceptions
        //  - More
        public static HttpResponseMessage Send(HttpRequestMessage request) {
            if(!request.HasHeader(HttpUserAgentHeader.NAME))
                request.SetHeader(HttpUserAgentHeader.NAME, USER_AGENT);

            request.SetHeader(HttpConnectionHeader.NAME, HttpConnectionHeader.KEEP_ALIVE);

            IPAddress[] addrs = Dns.GetHostAddresses(request.Host);

            if(!addrs.Any())
                throw new Exception(@"No addresses found for this host.");

            Exception exception = null;
            HttpClientConnection conn = null;

            foreach(IPAddress addr in addrs) {
                IPEndPoint endPoint = new IPEndPoint(addr, request.Port);

                exception = null;
                conn = new HttpClientConnection(request.Host, endPoint, request.IsSecure);
                conn.Acquire();

                try {
                    request.WriteTo(conn.Stream);
                } catch(IOException ex) {
                    exception = ex;
                    conn?.Dispose(); // temp
                    continue;
                } finally {
                    conn.Release();
                }
            }

            if(conn == null)
                throw new Exception(@"No connection.");

            HttpResponseMessage response = HttpResponseMessage.ReadFrom(conn.Stream);

            conn.Dispose(); // temp

            if(exception != null)
                throw exception;
            if(response == null)
                throw new Exception(@"Request failed.");
            return response;
        }
    }
}
