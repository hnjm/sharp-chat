using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharpChat.Flashii {
    public class FlashiiAuthRequest {
        [JsonPropertyName(@"user_id")]
        public long UserId { get; set; }

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
        public long UserId { get; set; }

        [JsonPropertyName(@"username")]
        public string Username { get; set; }

        [JsonPropertyName(@"colour_raw")]
        public int ColourRaw { get; set; }

        [JsonPropertyName(@"hierarchy")]
        public int Rank { get; set; }

        [JsonPropertyName(@"is_silenced")]
        public DateTimeOffset SilencedUntil { get; set; }

        [JsonPropertyName(@"perms")]
        public ChatUserPermissions Permissions { get; set; }

        public static async Task<FlashiiAuth> Attempt(HttpClient httpClient, FlashiiAuthRequest authRequest) {
            if(httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));
            if(authRequest == null)
                throw new ArgumentNullException(nameof(authRequest));

#if DEBUG
            if (authRequest.UserId >= 10000)
                return new FlashiiAuth {
                    Success = true,
                    UserId = authRequest.UserId,
                    Username = @"Misaka-" + (authRequest.UserId - 10000),
                    ColourRaw = (RNG.Next(0, 255) << 16) | (RNG.Next(0, 255) << 8) | RNG.Next(0, 255),
                    Rank = 0,
                    SilencedUntil = DateTimeOffset.MinValue,
                    Permissions = ChatUserPermissions.SendMessage | ChatUserPermissions.EditOwnMessage | ChatUserPermissions.DeleteOwnMessage,
                };
#endif

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, FlashiiUrls.AUTH) {
                Content = new ByteArrayContent(authRequest.GetJSON()),
                Headers = {
                    { @"X-SharpChat-Signature", authRequest.Hash },
                },
            };

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            return JsonSerializer.Deserialize<FlashiiAuth>(
                await response.Content.ReadAsByteArrayAsync()
            );
        }
    }
}
