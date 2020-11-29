using SharpChat.Users.Auth;

namespace SharpChat.DataProvider.Null {
    public class NullUserAuthClient : IUserAuthClient {
        public IUserAuthResponse AttemptAuth(UserAuthRequest request) {
            if(request.Token.StartsWith(@"FAIL:"))
                throw new UserAuthFailedException(request.Token[5..]);
            return new NullUserAuthResponse(request);
        }
    }
}
