namespace SharpChat.Users {
    public class ChatBot : IUser {
        public long UserId { get; } = -1;
        public string UserName { get; } = @"ChatBot";
        public Colour Colour { get; } = new Colour(0x9E8DA7);
        public int Rank { get; } = 0;
        public string NickName => null;
        public UserPermissions Permissions { get; } = (UserPermissions)(-1);
        public UserStatus Status => UserStatus.Online;
        public string StatusMessage => string.Empty;

        public string DisplayName => UserName;

        public bool Can(UserPermissions perm)
            => true;

        public string Pack() // permission part is empty for bot apparently
            => string.Join(IServerPacket.SEPARATOR, UserId, DisplayName, Colour, string.Empty);

        public override string ToString() {
            return @"<ChatBot>";
        }
    }
}
