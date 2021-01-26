using Hamakaze;
using SharpChat.Bans;
using SharpChat.Configuration;
using SharpChat.Users.Auth;
using SharpChat.Users.Bump;

namespace SharpChat.DataProvider.Null {
    [DataProvider(@"null")]
    public class NullDataProvider : IDataProvider {
        public IBanClient BanClient { get; }
        public IUserAuthClient UserAuthClient { get; }
        public IUserBumpClient UserBumpClient { get; }

#pragma warning disable IDE0060 // Remove unused parameter
        public NullDataProvider(IConfig config = null, HttpClient httpClient = null) {
#pragma warning restore IDE0060 // Remove unused parameter
            BanClient = new NullBanClient();
            UserAuthClient = new NullUserAuthClient();
            UserBumpClient = new NullUserBumpClient();
        }
    }
}
