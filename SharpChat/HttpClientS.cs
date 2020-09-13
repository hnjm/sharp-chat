using System;
using System.Net.Http;

namespace SharpChat {
    public class HttpClientS : HttpClient {
        private static HttpClientS InstanceValue;
        public static HttpClientS Instance {
            get {
                if (InstanceValue == null)
#pragma warning disable IDE0067
                    new HttpClientS();
#pragma warning restore IDE0067
                return InstanceValue;
            }
        }

        private void SetInstance() {
            if (InstanceValue != null)
                throw new Exception(@"An instance of HttpClient already exists.");
            InstanceValue = this;
        }

        public HttpClientS() : base() {
            SetInstance();
        }
        public HttpClientS(HttpMessageHandler handler) : base(handler) {
            SetInstance();
        }
        public HttpClientS(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler) {
            SetInstance();
        }
    }
}
