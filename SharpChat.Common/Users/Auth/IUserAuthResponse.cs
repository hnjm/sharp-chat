using System;

namespace SharpChat.Users.Auth {
    public interface IUserAuthResponse {
        long UserId { get; }
        string Username { get; }
        int Rank { get; }
        ChatColour Colour { get; }
        DateTimeOffset SilencedUntil { get; }
        ChatUserPermissions Permissions { get; }
    }
}
