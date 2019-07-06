using Newtonsoft.Json;
using System.Net;

namespace SquidChat
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

        public static FlashiiAuth Attempt(int userId, string token, string ip, string endpoint = "https://flashii.net/_sockchat.php?user_id={0}&token={1}&ip={2}")
        {
            try
            {
                using (WebClient wc = new WebClient())
                    return JsonConvert.DeserializeObject<FlashiiAuth>(wc.DownloadString(string.Format(endpoint, userId, token, ip)));
            }
            catch
            {
                return new FlashiiAuth { Success = false };
            }
        }
    }
}
