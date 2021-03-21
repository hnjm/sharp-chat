using SharpChat.Users;

namespace SharpChat.Events {
    public class UserUpdateEvent : Event {
        public const string TYPE = @"user:update";

        public override string Type => TYPE;
        public string UserName { get; }
        public Colour? Colour { get; }
        public int? Rank { get; }
        public string NickName { get; }
        public UserPermissions? Perms { get; }
        public UserStatus? Status { get; }
        public string StatusMessage { get; }

        public bool HasUserName => UserName != null;
        public bool HasNickName => NickName != null;
        public bool HasStatusMessage => StatusMessage != null;

        public UserUpdateEvent(
            IUser user,
            string userName = null,
            Colour? colour = null,
            int? rank = null,
            string nickName = null,
            UserPermissions? perms = null,
            UserStatus? status = null,
            string statusMessage = null
        ) : base(null, user) {
            UserName = userName;
            Colour = colour;
            Rank = rank;
            NickName = nickName;
            Perms = perms;
            Status = status;
            StatusMessage = statusMessage;
        }
    }
}
