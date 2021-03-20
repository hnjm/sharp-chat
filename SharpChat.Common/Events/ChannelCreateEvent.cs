using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class ChannelCreateEvent : Event {
        public const string TYPE = @"channel:create";

        public override string Type => TYPE;
        public string Name { get; }
        public bool IsTemporary { get; }
        public int MinimumRank { get; }
        public string Password { get; }
        public bool AutoJoin { get; }
        public uint MaxCapacity { get; }

        public ChannelCreateEvent(IEventTarget context, IUser user, string name, bool temp, int minRank, string password, bool autoJoin, uint maxCapacity)
            : base(context, user) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsTemporary = temp;
            MinimumRank = minRank;
            Password = password ?? string.Empty;
            AutoJoin = autoJoin;
            MaxCapacity = maxCapacity;
        }
        public ChannelCreateEvent(IEventTarget context, IChannel channel)
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
    }
}
