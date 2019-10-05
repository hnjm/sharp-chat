using SharpChat.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat {
    public class ChannelException : Exception { }
    public class ChannelExistException : ChannelException { }
    public class ChannelInvalidNameException : ChannelException { }

    public class ChannelManager : IDisposable {
        private readonly List<ChatChannel> Channels = new List<ChatChannel>();

        public readonly ChatContext Context;

        public bool IsDisposed { get; private set; }

        public ChannelManager(ChatContext context) {
            Context = context;
        }

        private ChatChannel _DefaultChannel;

        public ChatChannel DefaultChannel {
            get {
                if (_DefaultChannel == null)
                    _DefaultChannel = Channels.FirstOrDefault();

                return _DefaultChannel;
            }
            set {
                if (value == null)
                    return;

                if (Channels.Contains(value))
                    _DefaultChannel = value;
            }
        }


        public void Add(ChatChannel channel) {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            if (!channel.Name.All(c => char.IsLetter(c) || char.IsNumber(c) || c == '-'))
                throw new ChannelInvalidNameException();
            if (Get(channel.Name) != null)
                throw new ChannelExistException();

            // Add channel to the listing
            Channels.Add(channel);

            // Set as default if there's none yet
            if (_DefaultChannel == null)
                _DefaultChannel = channel;

            // Broadcast creation of channel
            foreach (ChatUser user in Context.Users.OfHierarchy(channel.Hierarchy))
                user.Send(new ChannelCreatePacket(channel));
        }

        public void Remove(ChatChannel channel) {
            if (channel == null || channel == DefaultChannel)
                return;

            // Remove channel from the listing
            Channels.Remove(channel);

            // Move all users back to the main channel
            // TODO: Replace this with a kick. SCv2 supports being in 0 channels, SCv1 should force the user back to DefaultChannel.
            foreach (ChatUser user in channel.GetUsers()) {
                Context.SwitchChannel(user, DefaultChannel, string.Empty);
            }

            // Broadcast deletion of channel
            foreach (ChatUser user in Context.Users.OfHierarchy(channel.Hierarchy))
                user.Send(new ChannelDeletePacket(channel));
        }

        public bool Contains(ChatChannel chan) {
            if (chan == null)
                return false;

            lock (Channels)
                return Channels.Contains(chan) || Channels.Any(c => c.Name.ToLowerInvariant() == chan.Name.ToLowerInvariant());
        }

        public void Update(ChatChannel channel, string name = null, bool? temporary = null, int? hierarchy = null, string password = null) {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            if (!Channels.Contains(channel))
                throw new ArgumentException(@"Provided channel is not registered with this manager.", nameof(channel));

            string prevName = channel.Name;
            int prevHierarchy = channel.Hierarchy;
            bool nameUpdated = !string.IsNullOrWhiteSpace(name) && name != prevName;

            if (nameUpdated) {
                if (!name.All(c => char.IsLetter(c) || char.IsNumber(c) || c == '-'))
                    throw new ChannelInvalidNameException();
                if (Get(name) != null)
                    throw new ChannelExistException();

                channel.Name = name;
            }

            if (temporary.HasValue)
                channel.IsTemporary = temporary.Value;

            if (hierarchy.HasValue)
                channel.Hierarchy = hierarchy.Value;

            if (password != null)
                channel.Password = password;

            // Users that no longer have access to the channel/gained access to the channel by the hierarchy change should receive delete and create packets respectively
            foreach (ChatUser user in Context.Users.OfHierarchy(channel.Hierarchy)) {
                user.Send(new ChannelUpdatePacket(prevName, channel));

                if (nameUpdated)
                    user.ForceChannel();
            }
        }

        public ChatChannel Get(string name) {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return Channels.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
        }

        public IEnumerable<ChatChannel> GetUser(ChatUser user) {
            if (user == null)
                return null;

            return Channels.Where(x => x.HasUser(user));
        }

        public IEnumerable<ChatChannel> OfHierarchy(int hierarchy) {
            lock (Channels)
                return Channels.Where(c => c.Hierarchy >= hierarchy).ToList();
        }

        ~ChannelManager()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing) {
            if (IsDisposed)
                return;
            IsDisposed = true;

            Channels.Clear();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
