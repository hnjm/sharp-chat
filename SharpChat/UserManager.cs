using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpChat
{
    public class UserManager : IDisposable, IEnumerable<ChatUser>
    {
        private readonly List<ChatUser> Users = new List<ChatUser>();

        public bool IsDisposed { get; private set; }

        public UserManager()
        {
            //
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
