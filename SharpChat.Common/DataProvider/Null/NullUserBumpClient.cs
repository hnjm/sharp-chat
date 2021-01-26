using SharpChat.Users;
using SharpChat.Users.Bump;
using System;
using System.Collections.Generic;

namespace SharpChat.DataProvider.Null {
    public class NullUserBumpClient : IUserBumpClient {
        public void SubmitBumpUsers(IEnumerable<ChatUser> users, Action onSuccess = null, Action<Exception> onFailure = null) {
            onSuccess?.Invoke();
        }
    }
}
