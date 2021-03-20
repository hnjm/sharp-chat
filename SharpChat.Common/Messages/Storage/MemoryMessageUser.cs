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
            UserId = mce.Sender.UserId;
            UserName = mce.Sender.UserName;
            Colour = mce.Sender.Colour;
            Rank = mce.Sender.Rank;
            NickName = mce.Sender.NickName;
            Permissions = mce.Sender.Permissions;
        }

        public bool Equals(IUser other)
            => other != null && other.UserId == UserId;
    }
}
