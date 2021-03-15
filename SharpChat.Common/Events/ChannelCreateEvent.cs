using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class ChannelCreateEvent : Event, IChannelEvent {
        public const string TYPE = @"channel:create";

        public override string Type => TYPE;
        public string Name { get; }
        public bool IsTemporary { get; }
        public int MinimumRank { get; }
        public string Password { get; }
        public bool AutoJoin { get; }
        public uint MaxCapacity { get; }

        private ChannelCreateEvent(IEvent evt, string name, bool temp, int minRank, string password, bool autoJoin, uint maxCapacity) : base(evt) {
            Name = name;
            IsTemporary = temp;
            MinimumRank = minRank;
            Password = password;
            AutoJoin = autoJoin;
            MaxCapacity = maxCapacity;
        }
        public ChannelCreateEvent(ChatContext context, IUser user, string name, bool temp, int minRank, string password, bool autoJoin, uint maxCapacity)
            : base(context, user) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsTemporary = temp;
            MinimumRank = minRank;
            Password = password ?? string.Empty;
            AutoJoin = autoJoin;
            MaxCapacity = maxCapacity;
        }
        public ChannelCreateEvent(ChatContext context, Channel channel)
            : base(context, channel.Owner) {
            Name = channel.Name;
            IsTemporary = channel.IsTemporary;
            MinimumRank = channel.MinimumRank;
            Password = channel.Password;
            AutoJoin = channel.AutoJoin;
            MaxCapacity = channel.MaxCapacity;
        }

        public override string EncodeAsJson() {
            return JsonSerializer.Serialize(new Dictionary<string, object> {
                { @"name", Name },
                { @"temp", IsTemporary },
                { @"rank", MinimumRank },
                { @"pass", Password },
                { @"auto", AutoJoin },
                { @"mcap", MaxCapacity },
            });
        }

        public static ChannelCreateEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            string name = string.Empty;
            if(elem.TryGetProperty(@"name", out JsonElement nameElem))
                name = nameElem.GetString();

            bool temp = false;
            if(elem.TryGetProperty(@"temp", out JsonElement tempElem))
                temp = tempElem.GetBoolean();

            int rank = 0;
            if(elem.TryGetProperty(@"rank", out JsonElement rankElem) && !rankElem.TryGetInt32(out rank))
                rank = 0;

            string password = null;
            if(elem.TryGetProperty(@"name", out JsonElement passwordElem))
                password = passwordElem.GetString();

            bool autoJoin = false;
            if(elem.TryGetProperty(@"auto", out JsonElement autoJoinElem))
                autoJoin = autoJoinElem.GetBoolean();

            uint maxCapacity = 0;
            if(elem.TryGetProperty(@"mcap", out JsonElement maxCapacityElem) && !maxCapacityElem.TryGetUInt32(out maxCapacity))
                maxCapacity = 0;

            return new ChannelCreateEvent(evt, name, temp, rank, password, autoJoin, maxCapacity);
        }
    }
}
