using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpChat.Flashii {
    public class FlashiiBan {
        public const string ENDPOINT =
#if DEBUG
            @"http://msz.lh/_sockchat/bans";
#else
            @"https://flashii.net/_sockchat/bans";
#endif

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
                using HttpRequestMessage bansRequest = new HttpRequestMessage(HttpMethod.Get, ENDPOINT);
                bansRequest.Headers.Add(@"X-SharpChat-Signature", @"givemethebeans".GetSignedHash());
                bansRequest.Headers.Add(@"User-Agent", @"SharpChat");
                using HttpResponseMessage bansResponse = HttpClientS.Instance.SendAsync(bansRequest).Result;
                return JsonSerializer.Deserialize<IEnumerable<FlashiiBan>>(bansResponse.Content.ReadAsByteArrayAsync().Result);
            } catch (Exception ex) {
                Logger.Write(ex);
                return null;
            }
        }
    }
}
