using System;

namespace SharpChat.Users.Auth {
    public interface IUserAuthResponse {
        long UserId { get; }
        string UserName { get; }
        int Rank { get; }
        Colour Colour { get; }
        DateTimeOffset SilencedUntil { get; }
        UserPermissions Permissions { get; }
    }
}
