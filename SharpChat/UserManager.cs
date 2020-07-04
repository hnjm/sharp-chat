using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat {
    public class UserManager : IDisposable {
        private readonly List<ChatUser> Users = new List<ChatUser>();

        public readonly ChatContext Context;

        public bool IsDisposed { get; private set; }

        public UserManager(ChatContext context) {
            Context = context;
        }

        public void Add(ChatUser user) {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            lock(Users)
                if(!Contains(user))
                    Users.Add(user);
        }

        public void Remove(ChatUser user) {
            if (user == null)
                return;

            lock(Users)
                Users.Remove(user);
        }

        public bool Contains(ChatUser user) {
            if (user == null)
                return false;

            lock (Users)
                return Users.Contains(user) || Users.Any(x => x.UserId == user.UserId || x.Username.ToLowerInvariant() == user.Username.ToLowerInvariant());
        }

        public ChatUser Get(long userId) {
            lock(Users)
                return Users.FirstOrDefault(x => x.UserId == userId);
        }

        public ChatUser Get(string username, bool includeNickName = true, bool includeV1Name = true) {
            if (string.IsNullOrWhiteSpace(username))
                return null;
            username = username.ToLowerInvariant();

            lock(Users)
                return Users.FirstOrDefault(x => x.Username.ToLowerInvariant() == username || (includeNickName && x.Nickname?.ToLowerInvariant() == username) || (includeV1Name && x.GetDisplayName(1).ToLowerInvariant() == username));
        }

        public IEnumerable<ChatUser> OfHierarchy(int hierarchy) {
            lock (Users)
                return Users.Where(u => u.Rank >= hierarchy).ToList();
        }

        public IEnumerable<ChatUser> WithActiveConnections() {
            lock (Users)
                return Users.Where(u => u.HasConnections).ToList();
        }

        public IEnumerable<ChatUser> All() {
            lock (Users)
                return Users.ToList();
        }
        
        ~UserManager()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing) {
            if (IsDisposed)
                return;
            IsDisposed = true;

            Users.Clear();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
