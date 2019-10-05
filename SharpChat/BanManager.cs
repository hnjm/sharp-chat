using SharpChat.Flashii;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat {
    public interface IBan {
        DateTimeOffset Expires { get; }
        string ToString();
    }

    public class BannedUser : IBan {
        public int UserId { get; set; }
        public DateTimeOffset Expires { get; set; }
        public string Username { get; set; }

        public BannedUser() {
        }

        public BannedUser(FlashiiBan fb) {
            UserId = fb.UserId;
            Expires = fb.Expires;
            Username = fb.Username;
        }

        public override string ToString() => Username;
    }

    public class BannedIPAddress : IBan {
        public IPAddress Address { get; set; }
        public DateTimeOffset Expires { get; set; }

        public BannedIPAddress() {
        }

        public BannedIPAddress(FlashiiBan fb) {
            Address = IPAddress.Parse(fb.UserIP);
            Expires = fb.Expires;
        }

        public override string ToString() => Address.ToString();
    }

    public class BanManager : IDisposable {
        private readonly List<IBan> BanList = new List<IBan>();

        public readonly ChatContext Context;

        public bool IsDisposed { get; private set; }

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

            if (!int.TryParse(username, out int userId))
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
            IEnumerable<FlashiiBan> bans = FlashiiBan.GetList().Where(x => x.Expires > DateTimeOffset.Now);

            if (!bans.Any())
                return;

            lock (BanList) {
                foreach (FlashiiBan fb in bans) {
                    if (!BanList.OfType<BannedUser>().Any(x => x.UserId == fb.UserId))
                        Add(new BannedUser(fb));
                    if (!BanList.OfType<BannedIPAddress>().Any(x => x.Address.ToString() == fb.UserIP))
                        Add(new BannedIPAddress(fb));
                }
            }
        }

        public IEnumerable<IBan> All() {
            lock (BanList)
                return BanList.ToList();
        }

        ~BanManager()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing) {
            if (IsDisposed)
                return;
            IsDisposed = true;

            BanList.Clear();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
