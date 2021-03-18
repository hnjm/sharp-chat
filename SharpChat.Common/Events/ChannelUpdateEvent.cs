using SharpChat.Channels;
using SharpChat.Users;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class ChannelUpdateEvent : Event {
        public const string TYPE = @"channel:update";

        public override string Type => TYPE;
        public string Name { get; }
        public bool? IsTemporary { get; }
        public int? MinimumRank { get; }
        public string Password { get; }
        public bool? AutoJoin { get; }
        public uint? MaxCapacity { get; }

        public bool HasName => Name != null;
        public bool HasPassword => Password != null;

        private ChannelUpdateEvent(IEvent evt, string name, bool? temp, int? minRank, string password, bool? autoJoin, uint? maxCapacity) : base(evt) {
            Name = name;
            IsTemporary = temp;
            MinimumRank = minRank;
            Password = password;
            AutoJoin = autoJoin;
            MaxCapacity = maxCapacity;
        }
        public ChannelUpdateEvent(IChannel channel, IUser user, string name = null, bool? temp = null, int? minRank = null, string password = null, bool? autoJoin = null, uint? maxCapacity = null)
            : base(channel, user) {
            Name = name;
            IsTemporary = temp;
            MinimumRank = minRank;
            Password = password;
            AutoJoin = autoJoin;
            MaxCapacity = maxCapacity;
        }

        public override string EncodeAsJson() {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if(HasName)
                data[@"name"] = Name;
            if(IsTemporary.HasValue)
                data[@"temp"] = IsTemporary.Value;
            if(MinimumRank.HasValue)
                data[@"rank"] = MinimumRank.Value;
            if(HasPassword)
                data[@"pass"] = Password;
            if(AutoJoin.HasValue)
                data[@"auto"] = AutoJoin.Value;
            if(MaxCapacity.HasValue)
                data[@"mcap"] = MaxCapacity.Value;
            return JsonSerializer.Serialize(data);
        }

        public static IEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            string name = null;
            if(elem.TryGetProperty(@"name", out JsonElement nameElem))
                name = nameElem.GetString();

            bool? temp = null;
            if(elem.TryGetProperty(@"temp", out JsonElement tempElem))
                temp = tempElem.GetBoolean();

            int? rank = null;
            if(elem.TryGetProperty(@"rank", out JsonElement rankElem) && rankElem.TryGetInt32(out int rankDecode))
                rank = rankDecode;

            string password = null;
            if(elem.TryGetProperty(@"pass", out JsonElement passwordElem))
                password = passwordElem.GetString();

            bool? autoJoin = null;
            if(elem.TryGetProperty(@"auto", out JsonElement autoJoinElem))
                autoJoin = autoJoinElem.GetBoolean();

            uint? maxCapacity = null;
            if(elem.TryGetProperty(@"mcap", out JsonElement maxCapacityElem) && maxCapacityElem.TryGetUInt32(out uint maxCapacityDecode))
                maxCapacity = maxCapacityDecode;

            return new ChannelUpdateEvent(evt, name, temp, rank, password, autoJoin, maxCapacity);
        }
    }
}
