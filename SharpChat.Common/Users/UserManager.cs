using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Users {
    public class UserManager {
        private List<ChatUser> Users { get; } = new List<ChatUser>();

        private ChatContext Context { get; }

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
                return Users.Contains(user) || Users.Any(x => x.UserId == user.UserId || x.UserName.ToLowerInvariant() == user.UserName.ToLowerInvariant());
        }

        public ChatUser Get(long userId) {
            lock(Users)
                return Users.FirstOrDefault(x => x.UserId == userId);
        }

        public ChatUser Get(string username, bool includeNickName = true, bool includeDisplayName = true) {
            if (string.IsNullOrWhiteSpace(username))
                return null;
            username = username.ToLowerInvariant();

            lock(Users)
                return Users.FirstOrDefault(x => x.UserName.ToLowerInvariant() == username
                    || (includeNickName && x.NickName?.ToLowerInvariant() == username)
                    || (includeDisplayName && x.DisplayName.ToLowerInvariant() == username));
        }

        public IEnumerable<ChatUser> OfHierarchy(int hierarchy) {
            lock (Users)
                return Users.Where(u => u.Rank >= hierarchy).ToList();
        }

        public IEnumerable<ChatUser> WithActiveConnections() {
            lock (Users)
                return Users.Where(u => Context.Sessions.GetSessionCount(u) > 0).ToList();
        }

        public IEnumerable<ChatUser> All() {
            lock (Users)
                return Users.ToList();
        }
    }
}
