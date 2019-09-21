using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharpChat.Flashii
{
    public class FlashiiBan
    {
        [JsonProperty(@"id")]
        public int UserId { get; set; }

        [JsonProperty(@"ip")]
        public string UserIP { get; set; }

        [JsonProperty(@"expires")]
        public DateTimeOffset Expires { get; set; }

        public static IEnumerable<FlashiiBan> GetList()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string banListUrl = string.Format(
                        Utils.ReadFileOrDefault(@"bans_endpoint.txt", @"https://flashii.net/_sockchat.php?bans={0}"),
                        @"givemethebeans".GetSignedHash()
                    );

                    return JsonConvert.DeserializeObject<IEnumerable<FlashiiBan>>(wc.DownloadString(banListUrl));
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                return null;
            }
        }
    }
}
