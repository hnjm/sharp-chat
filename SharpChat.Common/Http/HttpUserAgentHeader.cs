using System;

namespace SharpChat.Http {
    public class HttpUserAgentHeader : HttpHeader {
        public const string NAME = @"User-Agent";

        public override string Name => NAME;
        public override object Value { get; }
        
        public HttpUserAgentHeader(string userAgent) {
            Value = userAgent ?? throw new ArgumentNullException(nameof(userAgent));
        }
    }
}
