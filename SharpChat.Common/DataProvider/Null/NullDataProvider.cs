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

        // TODO: get rid of the second arg when HttpClient has been dropkicked
        public NullDataProvider(IConfig _ = null, object __ = null) {
            BanClient = new NullBanClient();
            UserAuthClient = new NullUserAuthClient();
            UserBumpClient = new NullUserBumpClient();
        }
    }
}
