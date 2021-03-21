using SharpChat.Users;

namespace SharpChat.Events {
    public class UserConnectEvent : Event {
        public const string TYPE = @"user:connect";

        public override string Type => TYPE;
        public UserStatus Status { get; }
        public string StatusMessage { get; }

        public UserConnectEvent(IUser user)
            : base(null, user) {
            Status = user.Status;
            StatusMessage = user.StatusMessage;
        }
    }
}
