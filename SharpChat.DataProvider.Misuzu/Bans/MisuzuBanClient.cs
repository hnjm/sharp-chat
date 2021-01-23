using SharpChat.Bans;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

namespace SharpChat.DataProvider.Misuzu.Bans {
    public class MisuzuBanClient : IBanClient {
        private const string STRING = @"givemethebeans";

        private MisuzuDataProvider DataProvider { get; }
        private HttpClient HttpClient { get; }

        private const string URL = @"/bans";

        public MisuzuBanClient(MisuzuDataProvider dataProvider, HttpClient httpClient) {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IEnumerable<IBanRecord> GetBanList() {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, DataProvider.GetURL(URL)) {
                Headers = {
                    { @"X-SharpChat-Signature", DataProvider.GetSignedHash(STRING) },
                },
            };

            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;

            return JsonSerializer.Deserialize<IEnumerable<MisuzuBanRecord>>(response.Content.ReadAsByteArrayAsync().Result);
        }
    }
}
