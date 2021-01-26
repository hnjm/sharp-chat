using System;

namespace SharpChat.Users.Auth {
    public interface IUserAuthClient {
        void AttemptAuth(UserAuthRequest request, Action<IUserAuthResponse> onSuccess, Action<Exception> onFailure);
    }
}
