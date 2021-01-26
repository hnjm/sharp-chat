using SharpChat.Users.Auth;
using System;

namespace SharpChat.DataProvider.Null {
    public class NullUserAuthClient : IUserAuthClient {
        public void AttemptAuth(UserAuthRequest request, Action<IUserAuthResponse> onSuccess, Action<Exception> onFailure) {
            if(request.Token.StartsWith(@"FAIL:")) {
                onFailure.Invoke(new UserAuthFailedException(request.Token[5..]));
                return;
            }

            onSuccess.Invoke(new NullUserAuthResponse(request));
        }
    }
}
