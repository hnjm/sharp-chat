using SharpChat.Users;
using SharpChat.Users.Bump;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        public void SubmitBumpUsers(IEnumerable<ChatUser> users) {
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(!users.Any())
                return;

            byte[] data = JsonSerializer.SerializeToUtf8Bytes(users.Where(x => x.RemoteAddresses.Any()).Select(x => new MisuzuUserBumpInfo(x)));

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DataProvider.GetURL(URL)) {
                Content = new ByteArrayContent(data),
                Headers = {
                    { @"X-SharpChat-Signature", DataProvider.GetSignedHash(data) },
                }
            };

            HttpClient.SendAsync(request).Wait();
        }
    }
}
