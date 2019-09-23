using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat
{
    public class UserManager : IDisposable, IEnumerable<ChatUser>
    {
        private readonly List<ChatUser> Users = new List<ChatUser>();

        public readonly ChatContext Context;

        public bool IsDisposed { get; private set; }

        public UserManager(ChatContext context)
        {
            Context = context;
        }

        public void Add(ChatUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            lock (Users)
                Users.Add(user);
        }

        public void Remove(ChatUser user)
        {
            if (user == null)
                return;

            lock (Users)
                Users.Remove(user);
        }

        public ChatUser Get(int userId)
        {
            lock (Users)
                return Users.FirstOrDefault(x => x.UserId == userId);
        }

        public ChatUser Get(string username, bool includeNickName = true, bool includeV1Name = true)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;
            username = username.ToLowerInvariant();

            lock (Users)
                return Users.FirstOrDefault(x => x.Username.ToLowerInvariant() == username || (includeNickName && x.Nickname?.ToLowerInvariant() == username) || (includeV1Name && x.GetDisplayName(1).ToLowerInvariant() == username));
        }

        ~UserManager()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            IsDisposed = true;

            Users.Clear();

            if (disposing)
                GC.SuppressFinalize(this);
        }

        public IEnumerator<ChatUser> GetEnumerator() => Users.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Users.GetEnumerator();
    }
}
