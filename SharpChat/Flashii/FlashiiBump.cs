using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpChat.Flashii {
    public class FlashiiBump {
        [JsonPropertyName(@"id")]
        public long UserId { get; set; }

        [JsonPropertyName(@"ip")]
        public string UserIP { get; set; }

        public static void Submit(IEnumerable<ChatUser> users) {
            List<FlashiiBump> bups = users.Where(u => u.HasConnections).Select(x => new FlashiiBump { UserId = x.UserId, UserIP = x.RemoteAddresses.First().ToString() }).ToList();

            if (bups.Any())
                Submit(bups);
        }

        public static void Submit(IEnumerable<FlashiiBump> users) {
            try {
                byte[] bumpJson = JsonSerializer.SerializeToUtf8Bytes(users);
                using HttpRequestMessage bumpRequest = new HttpRequestMessage(HttpMethod.Post, FlashiiUrls.BUMP);
                bumpRequest.Headers.Add(@"X-SharpChat-Signature", bumpJson.GetSignedHash());
                bumpRequest.Headers.Add(@"User-Agent", @"SharpChat");
                bumpRequest.Content = new ByteArrayContent(bumpJson);
                using HttpResponseMessage bumpResponse = HttpClientS.Instance.SendAsync(bumpRequest).Result;
            } catch (Exception ex) {
                Logger.Write(ex);
            }
        }
    }
}
