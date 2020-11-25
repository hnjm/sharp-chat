using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Bans {
    public interface IBan {
        DateTimeOffset Expires { get; }
        string ToString();
    }

    public class BannedUser : IBan {
        public long UserId { get; set; }
        public DateTimeOffset Expires { get; set; }
        public string Username { get; set; }

        public BannedUser() {
        }

        public BannedUser(IBanRecord banRecord) {
            UserId = banRecord.UserId;
            Expires = banRecord.Expires;
            Username = banRecord.Username;
        }

        public override string ToString() => Username;
    }

    public class BannedIPAddress : IBan {
        public IPAddress Address { get; set; }
        public DateTimeOffset Expires { get; set; }

        public BannedIPAddress() {
        }

        public BannedIPAddress(IBanRecord banRecord) {
            Address = banRecord.UserIP;
            Expires = banRecord.Expires;
        }

        public override string ToString() => Address.ToString();
    }

    public class BanManager : IDisposable {
        private readonly List<IBan> BanList = new List<IBan>();

        public readonly ChatContext Context;

        public BanManager(ChatContext context) {
            Context = context;
            RefreshFlashiiBans();
        }

        public void Add(ChatUser user, DateTimeOffset expires) {
            if (expires <= DateTimeOffset.Now)
                return;

            lock (BanList) {
                BannedUser ban = BanList.OfType<BannedUser>().FirstOrDefault(x => x.UserId == user.UserId);

                if (ban == null)
                    Add(new BannedUser { UserId = user.UserId, Expires = expires, Username = user.Username });
                else
                    ban.Expires = expires;
            }
        }

        public void Add(IPAddress addr, DateTimeOffset expires) {
            if (expires <= DateTimeOffset.Now)
                return;

            lock (BanList) {
                BannedIPAddress ban = BanList.OfType<BannedIPAddress>().FirstOrDefault(x => x.Address.Equals(addr));

                if (ban == null)
                    Add(new BannedIPAddress { Address = addr, Expires = expires });
                else
                    ban.Expires = expires;
            }
        }

        private void Add(IBan ban) {
            if (ban == null)
                return;

            lock (BanList)
                if (!BanList.Contains(ban))
                    BanList.Add(ban);
        }

        public void Remove(ChatUser user) {
            lock(BanList)
                BanList.RemoveAll(x => x is BannedUser ub && ub.UserId == user.UserId);
        }

        public void Remove(IPAddress addr) {
            lock(BanList)
                BanList.RemoveAll(x => x is BannedIPAddress ib && ib.Address.Equals(addr));
        }

        public void Remove(IBan ban) {
            lock (BanList)
                BanList.Remove(ban);
        }

        public DateTimeOffset Check(ChatUser user) {
            if (user == null)
                return DateTimeOffset.MinValue;

            lock(BanList)
                return BanList.OfType<BannedUser>().Where(x => x.UserId == user.UserId).FirstOrDefault()?.Expires ?? DateTimeOffset.MinValue;
        }

        public DateTimeOffset Check(IPAddress addr) {
            if (addr == null)
                return DateTimeOffset.MinValue;

            lock (BanList)
                return BanList.OfType<BannedIPAddress>().Where(x => x.Address.Equals(addr)).FirstOrDefault()?.Expires ?? DateTimeOffset.MinValue;
        }

        public BannedUser GetUser(string username) {
            if (username == null)
                return null;

            if (!long.TryParse(username, out long userId))
                userId = 0;

            lock (BanList)
                return BanList.OfType<BannedUser>().FirstOrDefault(x => x.Username.ToLowerInvariant() == username.ToLowerInvariant() || (userId > 0 && x.UserId == userId));
        }

        public BannedIPAddress GetIPAddress(IPAddress addr) {
            lock (BanList)
                return BanList.OfType<BannedIPAddress>().FirstOrDefault(x => x.Address.Equals(addr));
        }

        public void RemoveExpired() {
            lock(BanList)
                BanList.RemoveAll(x => x.Expires <= DateTimeOffset.Now);
        }

        public void RefreshFlashiiBans() {
            IEnumerable<IBanRecord> bans;
            try {
                bans = Context.Server.DataProvider.BanClient.GetBanList();
            } catch(Exception ex) {
                Logger.Write($@"Ban Refresh: {ex.Message}");
                Logger.Write(ex);
                return;
            }

            if(!bans.Any())
                return;

            lock(BanList) {
                foreach(IBanRecord br in bans) {
                    if(!BanList.OfType<BannedUser>().Any(x => x.UserId == br.UserId))
                        Add(new BannedUser(br));
                    if(!BanList.OfType<BannedIPAddress>().Any(x => x.Address == br.UserIP))
                        Add(new BannedIPAddress(br));
                }
            }
        }

        public IEnumerable<IBan> All() {
            lock (BanList)
                return BanList.ToList();
        }

        private bool IsDisposed;

        ~BanManager()
            => DoDispose();

        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }

        private void DoDispose() {
            if (IsDisposed)
                return;
            IsDisposed = true;
            BanList.Clear();
        }
    }
}
