using System;
using System.Collections;
using System.Collections.Generic;

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
