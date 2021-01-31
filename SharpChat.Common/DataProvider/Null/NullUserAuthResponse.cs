using SharpChat.Users;
using SharpChat.Users.Auth;
using System;

namespace SharpChat.DataProvider.Null {
    public class NullUserAuthResponse : IUserAuthResponse {
        public long UserId { get; }
        public string Username { get; }
        public int Rank { get; }
        public Colour Colour { get; }
        public UserPermissions Permissions { get; }
        public DateTimeOffset SilencedUntil => DateTimeOffset.MinValue;

        public NullUserAuthResponse(UserAuthRequest uar) {
            UserId = uar.UserId;
            Username = $@"Misaka-{uar.UserId}";
            Rank = (int)(uar.UserId % 10);
            Random rng = new Random((int)uar.UserId);
            Colour = new Colour(rng.Next());
            Permissions = (UserPermissions)rng.Next();
        }
    }
}
