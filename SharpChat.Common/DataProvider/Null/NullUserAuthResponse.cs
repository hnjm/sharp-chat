using SharpChat.Users;
using SharpChat.Users.Auth;
using System;

namespace SharpChat.DataProvider.Null {
    public class NullUserAuthResponse : IUserAuthResponse {
        public long UserId { get; }
        public string Username { get; }
        public int Rank { get; }
        public ChatColour Colour { get; }
        public ChatUserPermissions Permissions { get; }
        public DateTimeOffset SilencedUntil => DateTimeOffset.MinValue;

        public NullUserAuthResponse(UserAuthRequest uar) {
            UserId = uar.UserId;
            Username = $@"Misaka-{uar.UserId}";
            Rank = (int)(uar.UserId % 10);
            Random rng = new Random((int)uar.UserId);
            Colour = new ChatColour(rng.Next());
            Permissions = (ChatUserPermissions)rng.Next();
        }
    }
}
