using SharpChat.Users;

namespace SharpChat.Events {
    public class UserDisconnectEvent : Event {
        public const string TYPE = @"user:disconnect";

        public override string Type => TYPE;
        public UserDisconnectReason Reason { get; }

        public UserDisconnectEvent(IUser user, UserDisconnectReason reason)
            : base(null, user) {
            Reason = reason;
        }
    }
}
