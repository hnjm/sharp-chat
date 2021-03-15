using SharpChat.Configuration;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Channels {
    public class ChannelException : Exception { }
    public class ChannelExistException : ChannelException { }
    public class ChannelInvalidNameException : ChannelException { }

    public class ChannelManager : IEventHandler {
        private List<Channel> Channels { get; } = new List<Channel>();

        private IConfig Config { get; }
        private CachedValue<string[]> ChannelNames { get; }

        private ChatContext Context { get; }
        private ChatBot Bot { get; }
        private object Sync { get; } = new object();

        public ChannelManager(ChatContext context, IConfig config) {
            if(context == null)
                throw new ArgumentNullException(nameof(config));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            ChannelNames = Config.ReadCached(@"channels", new[] { @"lounge" });
            Bot = context.Bot;
            UpdateConfigChannels();
            Context = context;
        }

        public Channel DefaultChannel { get; private set; }

        // Needs better name + figure out how to run periodically
        public void UpdateConfigChannels() {
            lock(Sync) {
                string[] channelNames = ChannelNames;

                foreach(Channel channel in Channels) {
                    if(channelNames.Contains(channel.Name)) {
                        using IConfig config = Config.ScopeTo($@"channels:{channel.Name}");
                        bool autoJoin = config.ReadValue(@"autoJoin", DefaultChannel == null || DefaultChannel == channel);
                        string password = null;
                        int? minRank = null;
                        uint? maxCapacity = null;

                        if(!autoJoin) {
                            password = config.ReadValue(@"password", string.Empty);
                            if(string.IsNullOrEmpty(password))
                                password = null;

                            minRank = config.SafeReadValue(@"minRank", 0);
                            maxCapacity = config.SafeReadValue(@"maxCapacity", 0u);
                        }

                        Update(channel, null, false, minRank, password, autoJoin, maxCapacity);
                    } else if(!channel.IsTemporary) // Not in config == temporary
                        Update(channel, temporary: true);
                }

                foreach(string channelName in channelNames) {
                    if(Channels.Any(x => x.Name == channelName))
                        continue;
                    using IConfig config = Config.ScopeTo($@"channels:{channelName}");
                    bool autoJoin = config.ReadValue(@"autoJoin", DefaultChannel == null || DefaultChannel.Name == channelName);
                    string password = null;
                    int minRank = 0;
                    uint maxCapacity = 0;

                    if(!autoJoin) {
                        password = config.ReadValue(@"password", string.Empty);
                        if(string.IsNullOrEmpty(password))
                            password = null;

                        minRank = config.SafeReadValue(@"minRank", 0);
                        maxCapacity = config.SafeReadValue(@"maxCapacity", 0u);
                    }

                    Create(Bot, channelName, false, minRank, password, autoJoin, maxCapacity);
                }

                if(DefaultChannel == null || DefaultChannel.IsTemporary || !channelNames.Contains(DefaultChannel.Name))
                    DefaultChannel = Channels.FirstOrDefault(c => !c.IsTemporary && c.AutoJoin);
            }
        }

        public void Add(Channel channel) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(!channel.Name.All(c => char.IsLetter(c) || char.IsNumber(c) || c == '-'))
                throw new ChannelInvalidNameException();
            if(Get(channel.Name) != null)
                throw new ChannelExistException();

            lock(Sync) {
                // Add channel to the listing
                Channels.Add(channel);

                // Set as default if there's none yet
                if(DefaultChannel == null)
                    DefaultChannel = channel;

                // Broadcast creation of channel
                if(Context != null)
                    foreach(ChatUser user in Context.Users.OfRank(channel.MinimumRank))
                        user.SendPacket(new ChannelCreatePacket(channel));
            }
        }

        public void Remove(Channel channel, IUser user = null) {
            if(channel == null || channel == DefaultChannel)
                return;

            lock(Sync) {
                // Broadcast death
                Context.HandleEvent(new ChannelRemoveEvent(channel, user ?? Context.Bot));

                // Remove channel from the listing
                Channels.Remove(channel);

                // Move all users back to the main channel
                // TODO:!!!!!!!!! Replace this with a kick. SCv2 supports being in 0 channels, SCv1 should force the user back to DefaultChannel.
                //foreach(ChatUser user in channel.GetUsers()) {
                //    Context.SwitchChannel(user, DefaultChannel);
                //}

                // Broadcast deletion of channel
                foreach(ChatUser u in Context.Users.OfRank(channel.MinimumRank))
                    u.SendPacket(new ChannelDeletePacket(channel));
            }
        }

        public bool Contains(Channel chan) {
            if(chan == null)
                return false;

            lock(Sync) {
                return Channels.Contains(chan) || Channels.Any(c => c.Name.ToLowerInvariant() == chan.Name.ToLowerInvariant());
            }
        }

        public void HandleEvent(IEvent evt) {
            if(evt is not IChannelEvent chanEvt)
                return;

            Get(chanEvt.Target)?.HandleEvent(chanEvt);
        }

        public Channel Create(IUser owner, string name, bool temp = true, int minRank = 0, string password = null, bool autoJoin = false, uint maxCapacity = 0) {
            Channel channel = new Channel(name, temp, minRank, password, autoJoin, maxCapacity, owner);
            Add(channel);

            Context?.HandleEvent(new ChannelCreateEvent(Context, channel));

            return channel;
        }

        public void Update(Channel channel, string name = null, bool? temporary = null, int? minRank = null, string password = null, bool? autoJoin = null, uint? maxCapacity = null) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(!Channels.Contains(channel))
                throw new ArgumentException(@"Provided channel is not registered with this manager.", nameof(channel));

            lock(Sync) {
                string prevName = channel.Name;
                int prevHierarchy = channel.MinimumRank;
                bool nameUpdated = !string.IsNullOrWhiteSpace(name) && name != prevName;

                if(nameUpdated) {
                    if(!name.All(c => char.IsLetter(c) || char.IsNumber(c) || c == '-'))
                        throw new ChannelInvalidNameException();
                    if(Get(name) != null)
                        throw new ChannelExistException();
                }

                Context.HandleEvent(new ChannelUpdateEvent(channel, Context.Bot, name, temporary, minRank, password, autoJoin, maxCapacity));

                // Users that no longer have access to the channel/gained access to the channel by the hierarchy change should receive delete and create packets respectively
                // TODO: should be moved to the usermanager probably
                foreach(ChatUser user in Context.Users.OfRank(channel.MinimumRank)) {
                    user.SendPacket(new ChannelUpdatePacket(prevName, channel));

                    if(nameUpdated)
                        user.ForceChannel();
                }
            }
        }

        public Channel Get(string name) {
            if(string.IsNullOrWhiteSpace(name))
                return null;

            lock(Sync) {
                return Channels.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            }
        }

        public IEnumerable<Channel> GetUser(ChatUser user) {
            if(user == null)
                return null;

            lock(Sync) {
                return Channels.Where(x => x.HasUser(user));
            }
        }

        public IEnumerable<Channel> OfHierarchy(int hierarchy) {
            lock(Sync) {
                return Channels.Where(c => c.MinimumRank <= hierarchy);
            }
        }
    }
}
