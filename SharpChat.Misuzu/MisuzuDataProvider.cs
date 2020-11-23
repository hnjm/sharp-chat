using SharpChat.Bans;
using SharpChat.Misuzu.Bans;
using SharpChat.Misuzu.Users.Auth;
using SharpChat.Misuzu.Users.Bump;
using SharpChat.Users.Auth;
using SharpChat.Users.Bump;
using System;
using System.Net.Http;

namespace SharpChat.Misuzu {
    public class MisuzuDataProvider : IDataProvider {
        private HttpClient HttpClient { get; }
        public IBanClient BanClient { get; }
        public IUserAuthClient UserAuthClient { get; }
        public IUserBumpClient UserBumpClient { get; }

        public MisuzuDataProvider(HttpClient httpClient) {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            BanClient = new MisuzuBanClient(HttpClient);
            UserAuthClient = new MisuzuUserAuthClient(HttpClient);
            UserBumpClient = new MisuzuUserBumpClient(HttpClient);
        }
    }
}
