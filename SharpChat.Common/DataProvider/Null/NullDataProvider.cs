using SharpChat.Bans;
using SharpChat.Users.Auth;
using SharpChat.Users.Bump;

namespace SharpChat.DataProvider.Null {
    public class NullDataProvider : IDataProvider {
        public IBanClient BanClient { get; }
        public IUserAuthClient UserAuthClient { get; }
        public IUserBumpClient UserBumpClient { get; }

        public NullDataProvider() {
            BanClient = new NullBanClient();
            UserAuthClient = new NullUserAuthClient();
            UserBumpClient = new NullUserBumpClient();
        }
    }
}
