using SharpChat.Users;
using SharpChat.Users.Bump;
using System.Collections.Generic;

namespace SharpChat.DataProvider.Null {
    public class NullUserBumpClient : IUserBumpClient {
        public void SubmitBumpUsers(IEnumerable<ChatUser> users) {}
    }
}
