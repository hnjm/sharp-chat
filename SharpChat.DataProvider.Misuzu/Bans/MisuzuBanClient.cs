using Hamakaze;
using SharpChat.Bans;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

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
            Logger.Debug(@"Fetching bans...");

            // ban fetch needs a bit of a structural rewrite to use callbacks instead of not
            using ManualResetEvent mre = new ManualResetEvent(false);

            using HttpRequestMessage req = new HttpRequestMessage(HttpRequestMessage.GET, DataProvider.GetURL(URL));
            req.SetHeader(@"X-SharpChat-Signature", DataProvider.GetSignedHash(STRING));

            IEnumerable<MisuzuBanRecord> bans = Enumerable.Empty<MisuzuBanRecord>();

            HttpClient.SendRequest(
                req,
                onComplete: (t, r) => {
                    Logger.Debug(@"Here!");
                    using MemoryStream ms = new MemoryStream();
                    r.Body.CopyTo(ms);
                    bans = JsonSerializer.Deserialize<IEnumerable<MisuzuBanRecord>>(ms.ToArray());
                    mre.Set();
                },
                onCancel: (t) => mre.Set(),
                onError: (t, e) => {
                    Logger.Write(@"An error occurred during ban list fetch.");
                    Logger.Debug(e);
                },
                onStateChange: (t, s) => Logger.Debug(s)
            );

            mre.WaitOne();

            return bans;
        }
    }
}
