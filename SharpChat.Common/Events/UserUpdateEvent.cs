using SharpChat.Users;
using System.Collections.Generic;
using System.Text.Json;

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
            IEventTarget target,
            IUser user,
            string userName = null,
            Colour? colour = null,
            int? rank = null,
            string nickName = null,
            UserPermissions? perms = null,
            UserStatus? status = null,
            string statusMessage = null
        ) : base(target, user) {
            UserName = userName;
            Colour = colour;
            Rank = rank;
            NickName = nickName;
            Perms = perms;
            Status = status;
            StatusMessage = statusMessage;
        }

        public override string EncodeAsJson() {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if(HasUserName)
                data[@"name"] = UserName;
            if(Colour.HasValue)
                data[@"col"] = Colour.Value.Raw;
            if(Rank.HasValue)
                data[@"rank"] = Rank.Value;
            if(HasNickName)
                data[@"nick"] = NickName;
            if(Perms.HasValue)
                data[@"perm"] = (int)Perms.Value;
            if(Status.HasValue)
                data[@"stat"] = (int)Status.Value;
            if(HasStatusMessage)
                data[@"statm"] = StatusMessage;
            return JsonSerializer.Serialize(data);
        }
    }
}
