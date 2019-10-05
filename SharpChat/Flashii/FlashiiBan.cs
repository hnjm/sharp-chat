using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpChat.Flashii {
    public class FlashiiBan {
        [JsonPropertyName(@"id")]
        public int UserId { get; set; }

        [JsonPropertyName(@"ip")]
        public string UserIP { get; set; }

        [JsonPropertyName(@"expires")]
        public DateTimeOffset Expires { get; set; }

        [JsonPropertyName(@"username")]
        public string Username { get; set; }

        public static IEnumerable<FlashiiBan> GetList() {
            try {
                string bansEndpoint = string.Format(@"https://flashii.net/_sockchat.php?bans={0}", @"givemethebeans".GetSignedHash());

                return JsonSerializer.Deserialize<IEnumerable<FlashiiBan>>(
                    HttpClientS.Instance.GetByteArrayAsync(bansEndpoint).Result
                );
            } catch (Exception ex) {
                Logger.Write(ex);
                return null;
            }
        }
    }
}
