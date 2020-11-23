using System.Collections.Generic;

namespace SharpChat.Users.Bump {
    public interface IUserBumpClient {
        void SubmitBumpUsers(IEnumerable<ChatUser> users);
    }
}
