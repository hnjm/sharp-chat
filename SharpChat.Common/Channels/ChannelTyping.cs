using SharpChat.Users;
using System;

namespace SharpChat.Channels {
    public class ChannelTyping {
        public static TimeSpan Lifetime { get; } = TimeSpan.FromSeconds(5);

        public ChatUser User { get; }
        public DateTimeOffset Started { get; }

        public bool HasExpired
            => DateTimeOffset.Now - Started > Lifetime;

        public ChannelTyping(ChatUser user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Started = DateTimeOffset.Now;
        }
    }
}
