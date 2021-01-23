using SharpChat;
using SharpChat.Http;
using System.IO;
using System.Text;
using static System.Console;

namespace HttpClientTest {
    public static class Program {
        public static void Main(string[] args) {
            HttpRequestMessage req = new HttpRequestMessage(HttpRequestMessage.GET, @"https://flashii.net/");

            HttpResponseMessage res = HttpClient.Send(req);

            if(res.HasBody) {
                string line;
                using StreamWriter sw = new StreamWriter(@"out.html", false, new UTF8Encoding(false));
                using StreamReader sr = new StreamReader(res.Body, new UTF8Encoding(false), false, leaveOpen: true);
                while((line = sr.ReadLine()) != null) {
                    //Logger.Debug(line);
                    sw.WriteLine(line);
                }
            }

            ReadLine();
        }
    }
}
