using SharpChat.Users;
using SharpChat.Users.Auth;
using System;
using System.Net.Http;
using System.Text.Json;

namespace SharpChat.DataProvider.Misuzu.Users.Auth {
    public class MisuzuUserAuthClient : IUserAuthClient {
        private HttpClient HttpClient { get; }

        public MisuzuUserAuthClient(HttpClient httpClient) {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IUserAuthResponse AttemptAuth(UserAuthRequest request) {
            if(request == null)
                throw new ArgumentNullException(nameof(request));

#if DEBUG
            if(request.UserId >= 10000)
                return new MisuzuUserAuthResponse {
                    Success = true,
                    UserId = request.UserId,
                    Username = @"Misaka-" + (request.UserId - 10000),
                    ColourRaw = (RNG.Next(0, 255) << 16) | (RNG.Next(0, 255) << 8) | RNG.Next(0, 255),
                    Rank = 0,
                    SilencedUntil = DateTimeOffset.MinValue,
                    Permissions = ChatUserPermissions.SendMessage | ChatUserPermissions.EditOwnMessage | ChatUserPermissions.DeleteOwnMessage,
                };
#endif
            MisuzuUserAuthRequest mar = new MisuzuUserAuthRequest(request);

            using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, MisuzuUrls.AUTH) {
                Content = new ByteArrayContent(mar.GetJSON()),
                Headers = {
                    { @"X-SharpChat-Signature", mar.Hash },
                },
            };
            using HttpResponseMessage response = HttpClient.SendAsync(req).Result;

            MisuzuUserAuthResponse muar = JsonSerializer.Deserialize<MisuzuUserAuthResponse>(
                response.Content.ReadAsByteArrayAsync().Result
            );

            if(!muar.Success)
                throw new UserAuthFailedException(muar.Reason);

            return muar;
        }
    }
}
