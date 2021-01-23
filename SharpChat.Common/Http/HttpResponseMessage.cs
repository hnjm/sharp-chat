using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpChat.Http {
    public class HttpResponseMessage : HttpMessage {
        public override string ProtocolVersion { get; }
        public int StatusCode { get; }
        public string StatusMessage { get; }

        public override IEnumerable<HttpHeader> Headers { get; }

        public override Stream Body { get; }

        public HttpResponseMessage(
            int statusCode, string statusMessage, string protocolVersion,
            IEnumerable<HttpHeader> headers, Stream body
        ) {
            ProtocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
            StatusCode = statusCode;
            StatusMessage = statusMessage ?? string.Empty;
            Headers = (headers ?? throw new ArgumentNullException(nameof(headers))).ToArray();
            Body = body;
        }

        public static HttpResponseMessage ReadFrom(Stream stream) {
            // ignore this function, it doesn't exist
            string readLine() {
                const byte cr = 13, lf = 10;
                StringBuilder sb = new StringBuilder();
                int byt;
                bool gotCR = false;

                for(; ; ) {
                    byt = stream.ReadByte();
                    if(byt == -1 && sb.Length == 0)
                        return null;

                    if(gotCR) {
                        if(byt == lf)
                            break;
                        sb.Append('\r');
                    }

                    gotCR = byt == cr;
                    if(!gotCR)
                        sb.Append((char)byt);
                }

                return sb.ToString();
            }
            
            long contentLength = -1;
            Stack<string> encodings = null;

            // Read initial header
            string line = readLine();
            if(line == null)
                throw new IOException(@"Failed to read initial HTTP header.");
            if(!line.StartsWith(@"HTTP/"))
                throw new IOException(@"Response is not a valid HTTP message.");
            string[] parts = line[5..].Split(' ', 3);
            if(!int.TryParse(parts.ElementAtOrDefault(1), out int statusCode))
                throw new IOException(@"Invalid HTTP status code format.");
            string protocolVersion = parts.ElementAtOrDefault(0);
            string statusMessage = parts.ElementAtOrDefault(2);

            // Read header key-value pairs
            List<HttpHeader> headers = new List<HttpHeader>();

            while((line = readLine()) != null) {
                if(string.IsNullOrWhiteSpace(line))
                    break;

                parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
                if(parts.Length < 2)
                    throw new IOException(@"Invalid HTTP header in response.");

                string hName = HttpHeader.NormaliseName(parts.ElementAtOrDefault(0) ?? string.Empty),
                    hValue = parts.ElementAtOrDefault(1);
                if(string.IsNullOrEmpty(hName))
                    throw new IOException(@"Invalid HTTP header name.");

                HttpHeader header = HttpHeader.Create(hName, hValue);

                Console.ForegroundColor = ConsoleColor.Green;
                if(header is HttpContentLengthHeader hclh)
                    contentLength = (long)hclh.Value;
                else if(header is HttpTransferEncodingHeader hteh)
                    encodings = new Stack<string>(hteh.Encodings);
                else if(header is HttpCustomHeader)
                    Console.ForegroundColor = ConsoleColor.Red;

                Logger.Debug(header);
                Console.ResetColor();

                headers.Add(header);
            }

            Stream body = null;

            // Read body
            if(encodings != null && encodings.Any() && encodings.Peek() == HttpTransferEncodingHeader.CHUNKED) {
                // oh no the poop is chunky
                encodings.Pop();
                body = new MemoryStream();

                while((line = readLine()) != null) {
                    Logger.Debug(line);
                    if(string.IsNullOrWhiteSpace(line))
                        break;
                    if(!int.TryParse(line, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int chunkLength))
                        throw new IOException(@"Failed to decode chunk length.");
                    Logger.Debug($@"Chunk size: {chunkLength}");
                    if(chunkLength == 0) // final chunk
                        break;
                    byte[] buffer = new byte[chunkLength]; // lets blindly trust the server, that's never gone wrong
                    // the comment above is probably on to something, rework this to use a fixed size buffer
                    Logger.Debug(@"Reading...");
                    stream.Read(buffer, 0, chunkLength);
                    Logger.Debug(@"Writing...");
                    body.Write(buffer, 0, chunkLength);
                    readLine();
                }
            } else if(contentLength != 0) {
                body = new MemoryStream();
                int readSize = 8192;
                byte[] buffer = new byte[readSize];
                int read; long contentRemaining = contentLength;

                while((read = stream.Read(buffer, 0, readSize)) > 0) {
                    body.Write(buffer, 0, read);

                    if(contentLength >= 0) {
                        contentRemaining -= read;
                        if(contentRemaining < 1)
                            break;
                        if(readSize > contentRemaining)
                            readSize = (int)contentRemaining;
                    }
                }
            }

            if(body != null)
                // Check if body is empty and null it again if so
                if(body.Length == 0) {
                    body.Dispose();
                    body = null;
                } else {
                    // TODO: implement decoding
                    if(encodings != null)
                        while(encodings.TryPop(out string encoding)) {
                            if(encoding == HttpTransferEncodingHeader.CHUNKED)
                                throw new IOException(@"Invalid use of chunked encoding type in Transfer-Encoding header.");

                            Logger.Debug($@"Decode {encoding}");
                        }

                    body.Seek(0, SeekOrigin.Begin);
                }

            return new HttpResponseMessage(statusCode, statusMessage, protocolVersion, headers, body);
        }
    }
}
