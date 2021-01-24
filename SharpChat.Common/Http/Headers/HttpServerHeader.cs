using System;

namespace SharpChat.Http.Headers {
    public class HttpServerHeader : HttpHeader {
        public const string NAME = @"Server";

        public override string Name => NAME;
        public override object Value { get; }

        public HttpServerHeader(string server) {
            Value = server ?? throw new ArgumentNullException(nameof(server));
        }
    }
}
