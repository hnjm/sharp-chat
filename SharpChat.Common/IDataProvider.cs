using SharpChat.Bans;
using SharpChat.Users.Auth;
using SharpChat.Users.Bump;

namespace SharpChat {
    public interface IDataProvider {
        IBanClient BanClient { get; }
        IUserAuthClient UserAuthClient { get; }
        IUserBumpClient UserBumpClient { get; }
    }
}
