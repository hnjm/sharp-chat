using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharpChat
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

                    wc.UploadValues(Utils.ReadFileOrDefault(@"bump_endpoint.txt", @"https://flashii.net/_sockchat.php"), new System.Collections.Specialized.NameValueCollection {
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
