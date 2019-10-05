using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpChat.Flashii {
    public class FlashiiBump {
        [JsonPropertyName(@"id")]
        public int UserId { get; set; }

        [JsonPropertyName(@"ip")]
        public string UserIP { get; set; }

        public static void Submit(IEnumerable<ChatUser> users) {
            List<FlashiiBump> bups = users.Where(u => u.HasConnections).Select(x => new FlashiiBump { UserId = x.UserId, UserIP = x.RemoteAddresses.First().ToString() }).ToList();

            if (bups.Any())
                Submit(bups);
        }

        public static void Submit(IEnumerable<FlashiiBump> users) {
            try {
                string bumpJson = JsonSerializer.Serialize(users);

                FormUrlEncodedContent bumpData = new FormUrlEncodedContent(new Dictionary<string, string> {
                    { @"bump", bumpJson },
                    { @"hash", bumpJson.GetSignedHash() },
                });

                HttpClientS.Instance.PostAsync(@"https://flashii.net/_sockchat.php", bumpData).Wait();
            } catch (Exception ex) {
                Logger.Write(ex);
            }
        }
    }
}
