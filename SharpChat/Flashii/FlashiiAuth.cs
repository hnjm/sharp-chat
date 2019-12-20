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

        public static FlashiiAuth Attempt(FlashiiAuthRequest request) {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

#if DEBUG
            if (request.UserId >= 10000)
                return new FlashiiAuth {
                    Success = true,
                    UserId = request.UserId,
                    Username = @"Misaka-" + (request.UserId - 10000),
                    ColourRaw = (RNG.Next(0, 255) << 16) | (RNG.Next(0, 255) << 8) | RNG.Next(0, 255),
                    Hierarchy = 0,
                    SilencedUntil = DateTimeOffset.MinValue,
                    Permissions = ChatUserPermissions.SendMessage | ChatUserPermissions.EditOwnMessage | ChatUserPermissions.DeleteOwnMessage,
                };
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
