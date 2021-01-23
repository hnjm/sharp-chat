using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Http {
    public class HttpResponseMessage : HttpMessage {
        public override string ProtocolVersion => throw new NotImplementedException();

        public override IEnumerable<HttpHeader> Headers => throw new NotImplementedException();

        public override Stream Body => throw new NotImplementedException();

        public static HttpResponseMessage ReadFrom(Stream stream) {
            throw new NotImplementedException();
        }
    }
}
