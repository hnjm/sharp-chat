using SharpChat.Sessions;
using System;
using System.Collections.Generic;

namespace SharpChat.Users.Bump {
    public interface IUserBumpClient {
        void SubmitBumpUsers(SessionManager sessions, IEnumerable<IUser> users, Action onSuccess = null, Action<Exception> onFailure = null);
    }
}
