using SharpChat.Bans;
using System;
using System.Net;
using System.Text.Json.Serialization;

namespace SharpChat.DataProvider.Misuzu.Bans {
    public class MisuzuBanRecord : IBanRecord {
        [JsonPropertyName(@"id")]
        public long UserId { get; set; }

        [JsonIgnore]
        public IPAddress UserIP => IPAddress.Parse(UserIPString);

        [JsonPropertyName(@"ip")]
        public string UserIPString { get; set; }

        [JsonPropertyName(@"expires")]
        public DateTimeOffset Expires { get; set; }

        [JsonPropertyName(@"username")]
        public string Username { get; set; }

        [JsonIgnore]
        public bool IsPermanent { get; set; }
    }
}
