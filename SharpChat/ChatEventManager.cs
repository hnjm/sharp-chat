using SharpChat.Events;
using SharpChat.Packet;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpChat {
    public class ChatEventManager : IDisposable, IEnumerable<IChatEvent> {
        private readonly List<IChatEvent> Events = new List<IChatEvent>();

        public readonly ChatContext Context;

        public bool IsDisposed { get; private set; }

        public ChatEventManager(ChatContext context) {
            Context = context;
        }

        public void Add(IChatEvent evt) {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            lock (Events)
                Events.Add(evt);

            DB.LogEvent(evt);
        }

        public void Remove(IChatEvent evt) {
            if (evt == null)
                return;

            lock (Events)
                Events.Remove(evt);

            Context.Send(new ChatMessageDeletePacket(evt.SequenceId));
        }

        ~ChatEventManager()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing) {
            if (IsDisposed)
                return;
            IsDisposed = true;

            Events.Clear();

            if (disposing)
                GC.SuppressFinalize(this);
        }

        public IEnumerator<IChatEvent> GetEnumerator() => Events.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Events.GetEnumerator();
    }
}
