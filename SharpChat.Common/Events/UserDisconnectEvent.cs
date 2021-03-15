using SharpChat.Channels;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Text.Json;

namespace SharpChat.Events {
    public class UserDisconnectEvent : Event {
        public const string TYPE = @"user:disconnect";

        public override string Type => TYPE;
        public UserDisconnectReason Reason { get; }

        private UserDisconnectEvent(IEvent evt, UserDisconnectReason reason) : base(evt) {
            Reason = reason;
        }

        public UserDisconnectEvent(DateTimeOffset parted, IUser user, Channel target, UserDisconnectReason reason)
            : base(parted, user, target) {
            Reason = reason;
        }

        public static UserDisconnectEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            UserDisconnectReason reason = UserDisconnectReason.Unknown;
            if(elem.TryGetProperty(@"reason", out JsonElement reasonElem) && reasonElem.TryGetInt32(out int intReason))
                reason = (UserDisconnectReason)intReason;

            return new UserDisconnectEvent(evt, reason);
        }
    }
}
