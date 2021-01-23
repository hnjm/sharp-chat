using System;

namespace SharpChat.Http {
    public class HttpHostHeader : HttpHeader {
        public const string NAME = @"Host";

        public override string Name => NAME;
        public override object Value { get; }

        public HttpHostHeader(string host) {
            Value = host;
        }
    }
}
