using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat
{
    public class ChannelManager : IDisposable, IEnumerable<ChatChannel>
    {
        private readonly List<ChatChannel> Channels = new List<ChatChannel>();

        public bool IsDisposed { get; private set; }

        public ChannelManager()
        {
            //
        }

        public void Add(ChatChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            lock (Channels)
                Channels.Add(channel);
        }

        public void Remove(ChatChannel channel)
        {
            if (channel == null)
                return;

            lock (Channels)
                Channels.Remove(channel);
        }

        public ChatChannel Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            lock (Channels)
                return Channels.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
        }

        public IEnumerable<ChatChannel> GetUser(ChatUser user)
        {
            if (user == null)
                return null;

            lock (Channels)
                return Channels.Where(x => x.HasUser(user));
        }

        ~ChannelManager()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            IsDisposed = true;

            Channels.Clear();

            if (disposing)
                GC.SuppressFinalize(this);
        }

        public IEnumerator<ChatChannel> GetEnumerator() => Channels.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Channels.GetEnumerator();
    }
}
