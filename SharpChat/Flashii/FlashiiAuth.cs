using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpChat.Flashii {
    public class FlashiiAuthRequest {
        [JsonPropertyName(@"user_id")]
        public int UserId { get; set; }

        [JsonPropertyName(@"token")]
        public string Token { get; set; }

        [JsonPropertyName(@"ip")]
        public string IPAddress { get; set; }

        [JsonIgnore]
        public string Hash
            => string.Join(@"#", UserId, Token, IPAddress).GetSignedHash();

        public byte[] GetJSON()
            => JsonSerializer.SerializeToUtf8Bytes(this);
    }

    public class FlashiiAuth {
        [JsonPropertyName(@"success")]
        public bool Success { get; set; }

        [JsonPropertyName(@"reason")]
        public string Reason { get; set; } = @"none";

        [JsonPropertyName(@"user_id")]
        public int UserId { get; set; }

        [JsonPropertyName(@"username")]
        public string Username { get; set; }

        [JsonPropertyName(@"colour_raw")]
        public int ColourRaw { get; set; }

        [JsonPropertyName(@"hierarchy")]
        public int Hierarchy { get; set; }

        [JsonPropertyName(@"is_silenced")]
        public DateTimeOffset SilencedUntil { get; set; }

        [JsonPropertyName(@"perms")]
        public ChatUserPermissions Permissions { get; set; }

        private static FlashiiAuth[] AllUsers = null;

        public static FlashiiAuth Attempt(FlashiiAuthRequest request) {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if(AllUsers == null)
                AllUsers = JsonSerializer.Deserialize<FlashiiAuth[]>(
                    HttpClientS.Instance.GetByteArrayAsync(@"https://secret.flashii.net/sc-all.php").Result
                );

#if DEBUG
            if (request.UserId >= 10000)
                return AllUsers[(request.UserId - 9999) % AllUsers.Length];
#endif

            try {
                using ByteArrayContent loginContent = new ByteArrayContent(request.GetJSON());
                using HttpRequestMessage loginRequest = new HttpRequestMessage(HttpMethod.Post, FlashiiUrls.AUTH) {
                    Content = loginContent,
                };
                loginRequest.Headers.Add(@"X-SharpChat-Signature", request.Hash);
                loginRequest.Headers.Add(@"User-Agent", @"SharpChat");
                using HttpResponseMessage loginResponse = HttpClientS.Instance.SendAsync(loginRequest).Result;
                return JsonSerializer.Deserialize<FlashiiAuth>(loginResponse.Content.ReadAsByteArrayAsync().Result);
            } catch (Exception ex) {
                Logger.Write(ex.ToString());
                return new FlashiiAuth { Success = false };
            }
        }
    }
}
