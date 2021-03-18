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

        private UserUpdateEvent(
            IEvent evt,
            string userName,
            Colour? colour,
            int? rank,
            string nickName,
            UserPermissions? perms,
            UserStatus? status,
            string statusMessage
        ) : base(evt) {
            UserName = userName;
            Colour = colour;
            Rank = rank;
            NickName = nickName;
            Perms = perms;
            Status = status;
            StatusMessage = statusMessage;
        }

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

        public static IEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            string userName = null;
            if(elem.TryGetProperty(@"name", out JsonElement userNameElem))
                userName = userNameElem.GetString();

            Colour? colour = null;
            if(elem.TryGetProperty(@"col", out JsonElement colourElem) && colourElem.TryGetInt32(out int colourRaw))
                colour = colourRaw;

            int? rank = null;
            if(elem.TryGetProperty(@"rank", out JsonElement rankElem) && rankElem.TryGetInt32(out int rankDecode))
                rank = rankDecode;

            string nickName = null;
            if(elem.TryGetProperty(@"nick", out JsonElement nickNameElem))
                nickName = nickNameElem.GetString();

            UserPermissions? perms = null;
            if(elem.TryGetProperty(@"perm", out JsonElement permsElem) && permsElem.TryGetInt32(out int permsRaw))
                perms = (UserPermissions)permsRaw;

            UserStatus? status = null;
            if(elem.TryGetProperty(@"stat", out JsonElement statusElem) && statusElem.TryGetInt32(out int statusRaw))
                status = (UserStatus)statusRaw;

            string statusMsg = null;
            if(elem.TryGetProperty(@"statm", out JsonElement statusMsgElem))
                statusMsg = statusMsgElem.GetString();

            return new UserUpdateEvent(evt, userName, colour, rank, nickName, perms, status, statusMsg);
        }
    }
}
