using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharpChat.Flashii {
    public class FlashiiBan {
        private const string STRING = @"givemethebeans";

        [JsonPropertyName(@"id")]
        public int UserId { get; set; }

        [JsonPropertyName(@"ip")]
        public string UserIP { get; set; }

        [JsonPropertyName(@"expires")]
        public DateTimeOffset Expires { get; set; }

        [JsonPropertyName(@"username")]
        public string Username { get; set; }

        public static async Task<IEnumerable<FlashiiBan>> GetList(HttpClient httpClient) {
            if(httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, FlashiiUrls.BANS) {
                Headers = {
                    { @"X-SharpChat-Signature", STRING.GetSignedHash() },
                },
            };

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            return JsonSerializer.Deserialize<IEnumerable<FlashiiBan>>(await response.Content.ReadAsByteArrayAsync());
        }
    }
}
