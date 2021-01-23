using System;
using System.IO;

namespace SharpChat.Http {
    public class HttpContentLengthHeader : HttpHeader {
        public const string NAME = @"Content-Length";

        public override string Name => NAME;
        public override object Value => Stream.Length;

        private Stream Stream { get; }

        public HttpContentLengthHeader(Stream stream) {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if(!stream.CanRead || !stream.CanSeek)
                throw new ArgumentException(@"Body must readable and seekable.", nameof(stream));
        }
    }
}
