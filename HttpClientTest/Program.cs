using SharpChat;
using SharpChat.Http;
using System.IO;
using System.Text;
using static System.Console;

namespace HttpClientTest {
    public static class Program {
        public static void Main(string[] args) {
            HttpClient.DefaultUserAgent = @"SharpChat/1.0";

            /*string[] commonMediaTypes = new[] {
                @"application/x-executable",
                @"application/graphql",
                @"application/javascript",
                @"application/x.fwif",
                @"application/json",
                @"application/ld+json",
                @"application/msword",
                @"application/pdf",
                @"application/sql",
                @"application/vnd.api+json",
                @"application/vnd.ms-excel",
                @"application/vnd.ms-powerpoint",
                @"application/vnd.oasis.opendocument.text",
                @"application/vnd.openxmlformats-officedocument.presentationml.presentation",
                @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                @"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                @"application/x-www-form-urlencoded",
                @"application/xml",
                @"application/zip",
                @"application/zstd",
                @"audio/mpeg",
                @"audio/ogg",
                @"image/gif",
                @"image/apng",
                @"image/flif",
                @"image/webp",
                @"image/x-mng",
                @"image/jpeg",
                @"image/png",
                @"multipart/form-data",
                @"text/css",
                @"text/csv",
                @"text/html",
                @"text/php",
                @"text/plain",
                @"text/xml",
                @"text/html; charset=utf-8",
            };

            Logger.Write(@"Testing Media Type parsing...");
            foreach(string mts in commonMediaTypes) {
                HttpMediaType hmt = HttpMediaType.Parse(mts);
                Logger.Write($@"O {mts}");
                Logger.Write($@"P {hmt}");
            }

            return;*/

            HttpRequestMessage req = new HttpRequestMessage(HttpRequestMessage.GET, @"https://flashii.net/");
            WriteLine($@"Connection: {req.Connection}");
            WriteLine($@"AcceptEncodings: {string.Join(@", ", req.AcceptedEncodings)}");
            WriteLine($@"IsSecure: {req.IsSecure}");
            WriteLine($@"RequestTarget: {req.RequestTarget}");
            WriteLine($@"UserAgent: {req.UserAgent}");
            WriteLine($@"ContentType: {req.ContentType}");
            WriteLine();

            HttpResponseMessage res = HttpClient.Send(req);
            WriteLine($@"Connection: {res.Connection}");
            WriteLine($@"ContentEncodings: {string.Join(@", ", res.ContentEncodings)}");
            WriteLine($@"TransferEncodings: {string.Join(@", ", res.TransferEncodings)}");
            WriteLine($@"Date: {res.Date}");
            WriteLine($@"Server: {res.Server}");
            WriteLine($@"ContentType: {res.ContentType}");
            WriteLine();

            if(res.HasBody) {
                string line;
                using StreamWriter sw = new StreamWriter(@"out.html", false, new UTF8Encoding(false));
                using StreamReader sr = new StreamReader(res.Body, new UTF8Encoding(false), false, leaveOpen: true);
                while((line = sr.ReadLine()) != null) {
                    //Logger.Debug(line);
                    sw.WriteLine(line);
                }
            }
        }
    }
}
