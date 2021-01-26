using Hamakaze;
using SharpChat.Bans;
using System;
using System.Collections.Generic;
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

        public void GetBanList(Action<IEnumerable<IBanRecord>> onSuccess, Action<Exception> onFailure = null) {
            HttpRequestMessage req = new HttpRequestMessage(HttpRequestMessage.GET, DataProvider.GetURL(URL));
            req.SetHeader(@"X-SharpChat-Signature", DataProvider.GetSignedHash(STRING));

            HttpClient.SendRequest(
                req,
                onComplete: (t, r) => onSuccess.Invoke(JsonSerializer.Deserialize<IEnumerable<MisuzuBanRecord>>(r.GetBodyBytes())),
                onError: (t, e) => onFailure?.Invoke(e)
            );
        }
    }
}
