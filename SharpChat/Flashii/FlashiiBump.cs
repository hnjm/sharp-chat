using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharpChat.Flashii {
    public class FlashiiBump {
        [JsonPropertyName(@"id")]
        public long UserId { get; set; }

        [JsonPropertyName(@"ip")]
        public string UserIP { get; set; }

        public static void Submit(HttpClient httpClient, IEnumerable<ChatUser> users) {
            List<FlashiiBump> bups = users.Where(u => u.HasSessions).Select(x => new FlashiiBump { UserId = x.UserId, UserIP = x.RemoteAddresses.First().ToString() }).ToList();

            if (bups.Any())
                Submit(httpClient, bups);
        }

        public static void Submit(HttpClient httpClient, IEnumerable<FlashiiBump> users) {
            if(httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));
            if(users == null)
                throw new ArgumentNullException(nameof(users));
            if(!users.Any())
                return;

            byte[] data = JsonSerializer.SerializeToUtf8Bytes(users);

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, FlashiiUrls.BUMP) {
                Content = new ByteArrayContent(data),
                Headers = {
                    { @"X-SharpChat-Signature", data.GetSignedHash() },
                }
            };

            httpClient.SendAsync(request).ContinueWith(x => {
                if(x.IsFaulted)
                    Logger.Write($@"Flashii Bump Error: {x.Exception}");
            });
        }
    }
}
