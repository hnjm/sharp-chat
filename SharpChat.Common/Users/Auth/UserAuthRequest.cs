using System.Net;

namespace SharpChat.Users.Auth {
    public class UserAuthRequest {
        public long UserId { get; }
        public string Token { get; }
        public IPAddress RemoteAddress { get; }

        public UserAuthRequest(long userId, string token, IPAddress remoteAddress) {
            UserId = userId;
            Token = token;
            RemoteAddress = remoteAddress;
        }
    }
}
