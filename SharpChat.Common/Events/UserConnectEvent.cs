using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class UserConnectEvent : Event {
        public const string TYPE = @"user:connect";

        public UserStatus Status { get; }
        public string StatusMessage { get; }

        public UserConnectEvent(IUser user)
            : base(null, user ?? throw new ArgumentNullException(nameof(user))) {
            Status = user.Status;
            StatusMessage = user.StatusMessage;
        }
    }
}
