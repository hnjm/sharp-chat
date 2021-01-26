using Hamakaze;
using SharpChat.Users;
using SharpChat.Users.Auth;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace SharpChat.DataProvider.Misuzu.Users.Auth {
    public class MisuzuUserAuthClient : IUserAuthClient {
        private MisuzuDataProvider DataProvider { get; }
        private HttpClient HttpClient { get; }

        private const string URL = @"/verify";

        public MisuzuUserAuthClient(MisuzuDataProvider dataProvider, HttpClient httpClient) {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
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

            // auth needs a bit of a structural rewrite to use callbacks instead of not
            using ManualResetEvent mre = new ManualResetEvent(false);

            using HttpRequestMessage req = new HttpRequestMessage(HttpRequestMessage.POST, DataProvider.GetURL(URL));
            req.SetHeader(@"X-SharpChat-Signature", DataProvider.GetSignedHash(mar));
            req.SetBody(JsonSerializer.SerializeToUtf8Bytes(mar));

            MisuzuUserAuthResponse muar = null;

            HttpClient.SendRequest(
                req,
                onComplete: (t, r) => {
                    using MemoryStream ms = new MemoryStream();
                    r.Body.CopyTo(ms);
                    muar = JsonSerializer.Deserialize<MisuzuUserAuthResponse>(ms.ToArray());
                    mre.Set();
                },
                onCancel: (t) => mre.Set(),
                onError: (t, e) => {
                    Logger.Write(@"An error occurred during authentication.");
                    Logger.Debug(e);
                }
            );

            mre.WaitOne();

            if(muar?.Success != true)
                throw new UserAuthFailedException(muar.Reason);

            return muar;
        }
    }
}
