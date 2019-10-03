using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpChat.Flashii {
    public class FlashiiAuth {
        [JsonPropertyName(@"success")]
        public bool Success { get; set; }

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

        public static FlashiiAuth Attempt(int userId, string token, IPAddress ip) {
#if DEBUG
            if (userId >= 10000)
                return new FlashiiAuth {
                    Success = true,
                    UserId = userId,
                    Username = @"Misaka-" + (userId - 10000),
                    ColourRaw = (RNG.Next(0, 255) << 16) | (RNG.Next(0, 255) << 8) | RNG.Next(0, 255),
                    Hierarchy = 0,
                    SilencedUntil = DateTimeOffset.MinValue,
                    Permissions = 0,
                };
#endif

            try {
                using FormUrlEncodedContent loginRequest = new FormUrlEncodedContent(new Dictionary<string, string> {
                    { @"user_id", userId.ToString() },
                    { @"token", token },
                    { @"ip", ip.ToString() },
                    { @"hash", $@"{userId}#{token}#{ip}".GetSignedHash() },
                });

                using HttpResponseMessage loginResponse = HttpClientS.Instance.PostAsync(@"https://flashii.net/_sockchat.php", loginRequest).Result;

                return JsonSerializer.Deserialize<FlashiiAuth>(loginResponse.Content.ReadAsByteArrayAsync().Result);
            } catch (Exception ex) {
                Logger.Write(ex.ToString());
                return new FlashiiAuth { Success = false };
            }
        }
    }
}
