using System;

namespace SharpChat.Users.Auth {
    public class UserAuthFailedException : Exception {
        public UserAuthFailedException(string reason) : base(reason) { }
    }
}
