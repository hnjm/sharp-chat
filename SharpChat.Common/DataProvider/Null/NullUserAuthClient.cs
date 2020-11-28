using SharpChat.Users.Auth;

namespace SharpChat.DataProvider.Null {
    public class NullUserAuthClient : IUserAuthClient {
        public IUserAuthResponse AttemptAuth(UserAuthRequest request) {
            return new NullUserAuthResponse(request);
        }
    }
}
