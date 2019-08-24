using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SharpChat
{
    public class FlashiiAuth
    {
        [JsonProperty(@"success")]
        public bool Success { get; set; }

        [JsonProperty(@"user_id")]
        public int UserId { get; set; }

        [JsonProperty(@"username")]
        public string Username { get; set; }

        [JsonProperty(@"colour")]
        public string Colour { get; set; }

        [JsonProperty(@"colour_raw")]
        public int ColourRaw { get; set; }

        [JsonProperty(@"default_channel")]
        public string DefaultChannel { get; set; }

        [JsonProperty(@"hierarchy")]
        public int Hierarchy { get; set; }

        [JsonProperty(@"is_mod")]
        public bool IsModerator { get; set; }

        [JsonProperty(@"can_change_nick")]
        public bool CanChangeNick { get; set; }

        [JsonProperty(@"can_create_chan")]
        public SockChatUserChannel CanCreateChannels { get; set; }

        [JsonProperty(@"is_banned")]
        public DateTimeOffset BannedUntil { get; set; }

        [JsonProperty(@"is_silenced")]
        public DateTimeOffset SilencedUntil { get; set; }

        public static FlashiiAuth Attempt(int userId, string token, string ip)
        {
#if DEBUG
            if (userId >= 10000)
                return new FlashiiAuth
                {
                    Success = true,
                    UserId = userId,
                    Username = @"Misaka-" + (userId - 10000),
                    Colour = $@"rgb({RNG.Next(0, 255)}, {RNG.Next(0, 255)}, {RNG.Next(0, 255)})",
                    DefaultChannel = @"Lounge",
                    Hierarchy = 0,
                    IsModerator = false,
                    BannedUntil = DateTimeOffset.MinValue,
                    SilencedUntil = DateTimeOffset.MinValue,
                    CanChangeNick = false,
                    CanCreateChannels = SockChatUserChannel.No
                };
#endif

            try
            {
                using (WebClient wc = new WebClient())
                {
                    string authJson = Encoding.UTF8.GetString(wc.UploadValues(Utils.ReadFileOrDefault(@"login_endpoint.txt", @"https://flashii.net/_sockchat.php"), new NameValueCollection
                    {
                        { @"user_id", userId.ToString() },
                        { @"token", token },
                        { @"ip", ip },
                        { @"hash", $@"{userId}#{token}#{ip}".GetSignedHash() },
                    }));

                    return JsonConvert.DeserializeObject<FlashiiAuth>(authJson);
                }
            }
            catch(Exception ex)
            {
                Logger.Write(ex.ToString());
                return new FlashiiAuth { Success = false };
            }
        }
    }
}
