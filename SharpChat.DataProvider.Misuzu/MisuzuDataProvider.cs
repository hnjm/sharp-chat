using SharpChat.Bans;
using SharpChat.Configuration;
using SharpChat.DataProvider.Misuzu.Bans;
using SharpChat.DataProvider.Misuzu.Users.Auth;
using SharpChat.DataProvider.Misuzu.Users.Bump;
using SharpChat.Users.Auth;
using SharpChat.Users.Bump;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace SharpChat.DataProvider.Misuzu {
    [DataProvider(@"misuzu")]
    public class MisuzuDataProvider : IDataProvider, IDisposable {
        private HttpClient HttpClient { get; }
        private IConfig Config { get; }

        private CachedValue<string> SecretKey { get; }
        private CachedValue<string> BaseURL { get; }
        
        public IBanClient BanClient { get; }
        public IUserAuthClient UserAuthClient { get; }
        public IUserBumpClient UserBumpClient { get; }

        private const string DEFAULT_SECRET = @"woomy";

        public MisuzuDataProvider(IConfig config, HttpClient httpClient) {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            SecretKey = Config.ReadCached(@"secret", DEFAULT_SECRET, TimeSpan.FromMinutes(1));
            BaseURL = Config.ReadCached(@"endpoint", string.Empty, TimeSpan.FromMinutes(1));

            BanClient = new MisuzuBanClient(this, HttpClient);
            UserAuthClient = new MisuzuUserAuthClient(this, HttpClient);
            UserBumpClient = new MisuzuUserBumpClient(this, HttpClient);
        }

        public string GetURL(string path)
            => BaseURL.Value + path;

        public string GetSignedHash(object obj, string key = null)
            => GetSignedHash(obj.ToString(), key);

        public string GetSignedHash(string str, string key = null)
            => GetSignedHash(Encoding.UTF8.GetBytes(str), key);

        public string GetSignedHash(byte[] bytes, string key = null) {
            StringBuilder sb = new StringBuilder();

            using(HMACSHA256 algo = new HMACSHA256(Encoding.UTF8.GetBytes(key ?? SecretKey))) {
                byte[] hash = algo.ComputeHash(bytes);
                foreach(byte b in hash)
                    sb.AppendFormat(@"{0:x2}", b);
            }

            return sb.ToString();
        }

        private bool IsDisposed;
        ~MisuzuDataProvider()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;

            SecretKey.Dispose();
            BaseURL.Dispose();
        }
    }
}
