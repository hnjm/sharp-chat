using SharpChat.Users;
using System.Net;
using System.Text.Json.Serialization;

namespace SharpChat.DataProvider.Misuzu.Users.Bump {
    public class MisuzuUserBumpInfo {
        [JsonPropertyName(@"id")]
        public long UserId { get; }

        [JsonPropertyName(@"ip")]
        public string UserIP { get; }

        public MisuzuUserBumpInfo(IUser user, IPAddress addr) {
            UserId = user.UserId;
            UserIP = addr.ToString();
        }
    }
}
