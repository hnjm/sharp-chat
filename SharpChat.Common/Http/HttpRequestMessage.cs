using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpChat.Http {
    public class HttpRequestMessage : HttpMessage {
        public const string GET = @"GET";
        public const string POST = @"POST";

        public override string ProtocolVersion => @"1.1";

        public string Method { get; }
        public string RequestTarget { get; }

        public bool IsSecure { get; }

        public string Host { get; }

        public override IEnumerable<HttpHeader> Headers => HeaderList;
        private List<HttpHeader> HeaderList { get; } = new List<HttpHeader>();

        private bool OwnsBodyStream { get; set; }
        private Stream BodyStream { get; set; }
        public override Stream Body {
            get {
                if(BodyStream == null) {
                    OwnsBodyStream = true;
                    SetBody(new MemoryStream());
                }
                return BodyStream;
            }
        }

        private static readonly string[] HEADERS_READONLY = new[] {
            HttpHostHeader.NAME, HttpContentLengthHeader.NAME,
        };
        private static readonly string[] HEADERS_SINGLE = new[] {
            HttpUserAgentHeader.NAME,
        };

        public HttpRequestMessage(string method, string uri) : this(
            method, new Uri(uri)
        ) {}

        public HttpRequestMessage(string method, Uri uri) {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            RequestTarget = uri.PathAndQuery;
            IsSecure = uri.Scheme.Equals(@"https", StringComparison.InvariantCultureIgnoreCase);
            Host = uri.Host;
            HeaderList.Add(new HttpHostHeader(Host));
        }

        public static bool IsHeaderReadOnly(string name)
            => HEADERS_READONLY.Contains(name ?? throw new ArgumentNullException(nameof(name)));
        public static bool IsHeaderSingleInstance(string name)
            => HEADERS_SINGLE.Contains(name ?? throw new ArgumentNullException(nameof(name)));

        public void SetHeader(string name, object value) {
            name = HttpHeader.NormaliseName(name ?? throw new ArgumentNullException(nameof(name)));
            if(IsHeaderReadOnly(name))
                throw new ArgumentException(@"This header is read-only.", nameof(name));
            HeaderList.RemoveAll(x => x.Name == name);
            AddHeaderInternal(name, value);
        }

        public void AddHeader(string name, object value) {
            name = HttpHeader.NormaliseName(name ?? throw new ArgumentNullException(nameof(name)));
            if(IsHeaderReadOnly(name))
                throw new ArgumentException(@"This header is read-only.", nameof(name));
            if(IsHeaderSingleInstance(name))
                HeaderList.RemoveAll(x => x.Name == name);
            AddHeaderInternal(name, value);
        }

        private void AddHeaderInternal(string name, object value) {
            HttpHeader header = name switch {
                HttpUserAgentHeader.NAME => new HttpUserAgentHeader(value.ToString()),
                _ => new HttpCustomHeader(name, value),
            };

            HeaderList.Add(header);
        }

        public void RemoveHeader(string name) {
            name = HttpHeader.NormaliseName(name ?? throw new ArgumentNullException(nameof(name)));
            if(IsHeaderReadOnly(name))
                throw new ArgumentException(@"This header is read-only.", nameof(name));
            HeaderList.RemoveAll(x => x.Name == name);
        }

        public void SetBody(Stream stream) {
            if(stream == null) {
                if(OwnsBodyStream)
                    BodyStream?.Dispose();
                OwnsBodyStream = false;
                BodyStream = null;
                HeaderList.RemoveAll(x => x.Name == HttpContentLengthHeader.NAME);
            } else {
                if(!BodyStream.CanRead || !BodyStream.CanSeek)
                    throw new ArgumentException(@"Body must readable and seekable.", nameof(stream));
                if(OwnsBodyStream)
                    BodyStream?.Dispose();
                OwnsBodyStream = false;
                BodyStream = stream;
                HeaderList.Add(new HttpContentLengthHeader(BodyStream));
            }
        }

        public void WriteTo(Stream stream) {
            using(StreamWriter sw = new StreamWriter(stream, new ASCIIEncoding(), leaveOpen: true)) {
                sw.NewLine = "\r\n";
                sw.Write(Method);
                sw.Write(' ');
                sw.Write(RequestTarget);
                sw.Write(@" HTTP/");
                sw.WriteLine(ProtocolVersion);
                foreach(HttpHeader header in Headers)
                    sw.WriteLine(header);
                sw.WriteLine();
                sw.Flush();
            }
            BodyStream?.CopyTo(stream);
        }
    }
}
