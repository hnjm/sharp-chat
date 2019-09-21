using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace SharpChat.Flashii
{
    public class FlashiiBump
    {
        [JsonProperty(@"id")]
        public int UserId { get; set; }

        [JsonProperty(@"ip")]
        public string UserIP { get; set; }

        public static void Submit(IEnumerable<FlashiiBump> users)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string submitBump = JsonConvert.SerializeObject(users);

                    wc.UploadValues(Utils.ReadFileOrDefault(@"bump_endpoint.txt", @"https://flashii.net/_sockchat.php"), new NameValueCollection {
                        { @"bump", submitBump },
                        { @"hash", submitBump.GetSignedHash() },
                    });
                }
            }
            catch(Exception ex)
            {
                Logger.Write(ex);
            }
        }
    }
}
