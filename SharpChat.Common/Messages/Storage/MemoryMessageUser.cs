using SharpChat.Events;
using SharpChat.Users;

namespace SharpChat.Messages.Storage {
    public class MemoryMessageUser : IUser {
        public long UserId { get; }
        public string UserName { get; }
        public Colour Colour { get; }
        public int Rank { get; }
        public string NickName { get; }
        public UserPermissions Permissions { get; }
        public UserStatus Status => UserStatus.Unknown;
        public string StatusMessage => string.Empty;

        public MemoryMessageUser(MessageCreateEvent mce) {
            UserId = mce.User.UserId;
            UserName = mce.User.UserName;
            Colour = mce.User.Colour;
            Rank = mce.User.Rank;
            NickName = mce.User.NickName;
            Permissions = mce.User.Permissions;
        }

        public bool Equals(IUser other)
            => other != null && other.UserId == UserId;
    }
}
