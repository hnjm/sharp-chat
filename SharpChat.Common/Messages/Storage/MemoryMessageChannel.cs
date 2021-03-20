using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Users;
using System;

namespace SharpChat.Messages.Storage {
    public class MemoryMessageChannel : IChannel, IEventHandler {
        public string Name { get; private set; }
        public bool IsTemporary => true;
        public int MinimumRank => 0;
        public bool AutoJoin => false;
        public uint MaxCapacity => 0;
        public IUser Owner => null;
        public string Password => string.Empty;
        public bool HasPassword => false;
        public bool HasMaxCapacity => false;
        public string TargetName => Name.ToLowerInvariant();

        public MemoryMessageChannel(ChannelCreateEvent cce) {
            Name = cce.Target;
        }

        public MemoryMessageChannel(MessageCreateEvent evt) {
            Name = evt.Target;
        }

        public bool VerifyPassword(string password)
            => false;

        public bool Equals(IChannel other)
            => other != null && Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);

        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case ChannelUpdateEvent cue:
                    if(cue.HasName)
                        Name = cue.Name;
                    break;
            }
        }
    }
}
