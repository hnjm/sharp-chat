using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpChat
{
    public class ChatEventManager : IDisposable, IEnumerable<IChatMessage>
    {
        private readonly List<IChatMessage> Events = new List<IChatMessage>();

        public bool IsDisposed { get; private set; }

        public ChatEventManager()
        {
            //
        }

        public void Add(IChatMessage evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            lock (Events)
                Events.Add(evt);
        }

        public void Remove(IChatMessage evt)
        {
            if (evt == null)
                return;

            lock (Events)
                Events.Remove(evt);
        }

        ~ChatEventManager()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            IsDisposed = true;

            Events.Clear();

            if (disposing)
                GC.SuppressFinalize(this);
        }

        public IEnumerator<IChatMessage> GetEnumerator() => Events.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Events.GetEnumerator();
    }
}
