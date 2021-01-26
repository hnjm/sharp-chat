using Hamakaze;
using SharpChat.Users;
using SharpChat.Users.Bump;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SharpChat.DataProvider.Misuzu.Users.Bump {
    public class MisuzuUserBumpClient : IUserBumpClient {
        private MisuzuDataProvider DataProvider { get; }
        private HttpClient HttpClient { get; }

        private const string URL = @"/bump";

        public MisuzuUserBumpClient(MisuzuDataProvider dataProvider, HttpClient httpClient) {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void SubmitBumpUsers(IEnumerable<ChatUser> users, Action onSuccess = null, Action<Exception> onFailure = null) {
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(!users.Any())
                return;

            byte[] data = JsonSerializer.SerializeToUtf8Bytes(users.Where(x => x.RemoteAddresses.Any()).Select(x => new MisuzuUserBumpInfo(x)));

            HttpRequestMessage request = new HttpRequestMessage(HttpRequestMessage.POST, DataProvider.GetURL(URL));
            request.SetHeader(@"X-SharpChat-Signature", DataProvider.GetSignedHash(data));
            request.SetBody(data);

            HttpClient.SendRequest(
                request,
                disposeRequest: false,
                onComplete: (t, r) => { request.Dispose(); onSuccess?.Invoke(); },
                onError: (t, e) => {
                    Logger.Write(@"User bump request failed. Retrying once...");
                    Logger.Debug(e);

                    HttpClient.SendRequest(
                        request,
                        onComplete: (t, r) => {
                            Logger.Write(@"Second user bump attempt succeeded!");
                            onSuccess?.Invoke();
                        },
                        onError: (t, e) => {
                            Logger.Write(@"User bump request failed again.");
                            Logger.Debug(e);
                            onFailure?.Invoke(e);
                        }
                    );
                }
            );
        }
    }
}
