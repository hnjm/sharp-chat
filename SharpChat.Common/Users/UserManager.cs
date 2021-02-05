using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Users {
    public class UserManager {
        private List<ChatUser> Users { get; } = new List<ChatUser>();
        private ChatContext Context { get; }
        private object Sync { get; } = new object();

        public UserManager(ChatContext context) {
            Context = context;
        }

        public void Add(ChatUser user) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            lock(Sync)
                if(!Contains(user))
                    Users.Add(user);
        }

        public void Remove(ChatUser user) {
            if(user == null)
                return;

            lock(Sync)
                Users.Remove(user);
        }

        public bool Contains(ChatUser user) {
            if(user == null)
                return false;

            lock(Sync)
                return Users.Contains(user) || Users.Any(x => x.UserId == user.UserId || x.UserName.ToLowerInvariant() == user.UserName.ToLowerInvariant());
        }

        public ChatUser Get(long userId) {
            lock(Sync)
                return Users.FirstOrDefault(x => x.UserId == userId);
        }

        public ChatUser Get(string username, bool includeNickName = true, bool includeDisplayName = true) {
            if(string.IsNullOrWhiteSpace(username))
                return null;
            username = username.ToLowerInvariant();

            lock(Sync)
                return Users.FirstOrDefault(x => x.UserName.ToLowerInvariant() == username
                    || (includeNickName && x.NickName?.ToLowerInvariant() == username)
                    || (includeDisplayName && x.DisplayName.ToLowerInvariant() == username));
        }

        public IEnumerable<ChatUser> OfRank(int rank) {
            lock(Sync)
                return Users.Where(u => u.Rank >= rank).ToList();
        }

        public IEnumerable<ChatUser> WithActiveConnections() {
            lock(Sync)
                return Users.Where(u => Context.Sessions.GetSessionCount(u) > 0).ToList();
        }

        public IEnumerable<ChatUser> All() {
            lock(Sync)
                return Users.ToList();
        }
    }
}
