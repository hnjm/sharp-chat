namespace SharpChat.Users {
    public interface IUser {
        long UserId { get; }
        string UserName { get; }
        Colour Colour { get; }
        int Rank { get; }
        string NickName { get; }
        UserPermissions Permissions { get; }
        UserStatus Status { get; }
        string StatusMessage { get; }
        string DisplayName { get; }

        bool Can(UserPermissions perm);
        string Pack();
    }
}
