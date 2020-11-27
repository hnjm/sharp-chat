using SharpChat.Users;
using SharpChat.Users.Bump;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace SharpChat.Misuzu.Users.Bump {
    public class MisuzuUserBumpClient : IUserBumpClient {
        private HttpClient HttpClient { get; }

        public MisuzuUserBumpClient(HttpClient httpClient) {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void SubmitBumpUsers(IEnumerable<ChatUser> users) {
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(!users.Any())
                return;

            byte[] data = JsonSerializer.SerializeToUtf8Bytes(users.Where(x => x.RemoteAddresses.Any()).Select(x => new MisuzuUserBumpInfo(x)));

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, MisuzuUrls.BUMP) {
                Content = new ByteArrayContent(data),
                Headers = {
                    { @"X-SharpChat-Signature", data.GetSignedHash() },
                }
            };

            HttpClient.SendAsync(request).Wait();
        }
    }
}
