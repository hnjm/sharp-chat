using SharpChat.Events;
using SharpChat.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat {
    public class ChatEventManager : IDisposable {
        private readonly List<IChatEvent> Events = new List<IChatEvent>();

        public readonly ChatContext Context;

        public bool IsDisposed { get; private set; }

        public ChatEventManager(ChatContext context) {
            Context = context;
        }

        public void Add(IChatEvent evt) {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            lock(Events)
                Events.Add(evt);
        }

        public void Remove(IChatEvent evt) {
            if (evt == null)
                return;

            lock (Events)
                Events.Remove(evt);

            Context.Send(new ChatMessageDeletePacket(evt.SequenceId));
        }

        public IChatEvent Get(int seqId) {
            if (seqId < 1)
                return null;

            lock (Events)
                return Events.FirstOrDefault(e => e.SequenceId == seqId);
        }

        public IEnumerable<IChatEvent> GetTargetLog(IPacketTarget target, int amount = 20, int offset = 0) {
            lock (Events) {
                IEnumerable<IChatEvent> subset = Events.Where(e => e.Target == target || e.Target == null);

                int start = subset.Count() - offset - amount;

                if(start < 0) {
                    amount += start;
                    start = 0;
                }

                return subset.Skip(start).Take(amount).ToList();
            }
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
    }
}
