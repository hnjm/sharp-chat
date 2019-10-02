using SharpChat.Packet;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpChat {
    public class ChatEventManager : IDisposable, IEnumerable<IChatMessage> {
        private readonly List<IChatMessage> Events = new List<IChatMessage>();

        public readonly ChatContext Context;

        public bool IsDisposed { get; private set; }

        public ChatEventManager(ChatContext context) {
            Context = context;
        }

        public void Add(IChatMessage evt) {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            lock (Events)
                Events.Add(evt);
        }

        public void Remove(IChatMessage evt) {
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

        public IEnumerator<IChatMessage> GetEnumerator() => Events.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Events.GetEnumerator();
    }
}
