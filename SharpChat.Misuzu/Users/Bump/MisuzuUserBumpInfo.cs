using SharpChat.Users;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace SharpChat.Misuzu.Users.Bump {
    public class MisuzuUserBumpInfo {
        [JsonPropertyName(@"id")]
        public long UserId => User.UserId;

        [JsonPropertyName(@"ip")]
        public string UserIP => User.RemoteAddresses.First().ToString();

        private ChatUser User { get; }

        public MisuzuUserBumpInfo(ChatUser user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }
    }
}
