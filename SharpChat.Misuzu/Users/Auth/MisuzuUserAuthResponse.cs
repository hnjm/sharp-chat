using SharpChat.Users;
using SharpChat.Users.Auth;
using System;
using System.Text.Json.Serialization;

namespace SharpChat.Misuzu.Users.Auth {
    public class MisuzuUserAuthResponse : IUserAuthResponse {
        [JsonPropertyName(@"success")]
        public bool Success { get; set; }

        [JsonPropertyName(@"reason")]
        public string Reason { get; set; } = @"none";

        [JsonPropertyName(@"user_id")]
        public long UserId { get; set; }

        [JsonPropertyName(@"username")]
        public string Username { get; set; }

        [JsonPropertyName(@"colour_raw")]
        public int ColourRaw { get; set; }

        [JsonIgnore]
        public ChatColour Colour => new ChatColour(ColourRaw);

        [JsonPropertyName(@"hierarchy")]
        public int Rank { get; set; }

        [JsonPropertyName(@"is_silenced")]
        public DateTimeOffset SilencedUntil { get; set; }

        [JsonPropertyName(@"perms")]
        public ChatUserPermissions Permissions { get; set; }
    }
}
