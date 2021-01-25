using SharpChat.Http.Headers;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace SharpChat.Http {
    public static class HttpClient {
        public const string PRODUCT_STRING = @"HMKZ";
        public const string VERSION_MAJOR = @"1";
        public const string VERSION_MINOR = @"0";
        public const string USER_AGENT = PRODUCT_STRING + @"/" + VERSION_MAJOR + @"." + VERSION_MINOR;

        public static string DefaultUserAgent { get; set; } = USER_AGENT;

        // TODO
        //  - Threading
        //  - Proper exceptions
        //  - More
        public static HttpResponseMessage Send(HttpRequestMessage request) {
            // PREPARATION STEP
            if(string.IsNullOrWhiteSpace(request.UserAgent))
                request.UserAgent = DefaultUserAgent;

            //request.AcceptedEncodings = new[] { HttpEncoding.GZip };
            request.Connection = HttpConnectionHeader.CLOSE;

            // LOOKUP STEP
            IPAddress[] addrs = Dns.GetHostAddresses(request.Host);

            if(!addrs.Any())
                throw new Exception(@"No addresses found for this host.");

            // REQUEST STEP
            Exception exception = null;
            HttpClientConnection conn = null;

            foreach(IPAddress addr in addrs) {
                IPEndPoint endPoint = new IPEndPoint(addr, request.Port);

                Logger.Debug($@"Attempting {endPoint}...");

                exception = null;
                conn = new HttpClientConnection(request.Host, endPoint, request.IsSecure);
                conn.Acquire();

                try {
                    request.WriteTo(conn.Stream);
                    break;
                } catch(IOException ex) {
                    Logger.Debug(ex);
                    exception = ex;
                    conn?.Dispose(); // temp
                    continue;
                } finally {
                    conn.Release();
                }
            }

            if(conn == null)
                throw new Exception(@"No connection.");

            // RESPONSE STEP
            HttpResponseMessage response = HttpResponseMessage.ReadFrom(conn.Stream);

            conn.Dispose(); // temp

            // FINISHING STEP
            if(exception != null)
                throw exception;
            if(response == null)
                throw new Exception(@"Request failed.");
            return response;
        }
    }
}
