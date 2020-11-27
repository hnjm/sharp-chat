using SharpChat.Bans;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

namespace SharpChat.DataProvider.Misuzu.Bans {
    public class MisuzuBanClient : IBanClient {
        private const string STRING = @"givemethebeans";

        private HttpClient HttpClient { get; }

        public MisuzuBanClient(HttpClient httpClient) {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IEnumerable<IBanRecord> GetBanList() {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, MisuzuUrls.BANS) {
                Headers = {
                    { @"X-SharpChat-Signature", STRING.GetSignedHash() },
                },
            };

            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;

            return JsonSerializer.Deserialize<IEnumerable<MisuzuBanRecord>>(response.Content.ReadAsByteArrayAsync().Result);
        }
    }
}
