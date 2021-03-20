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
        private List<IChannel> Channels { get; } = new List<IChannel>();

        private IConfig Config { get; }
        private CachedValue<string[]> ChannelNames { get; }

        private IEventDispatcher Dispatcher { get; }
        private IEventTarget Target { get; }
        private UserManager Users { get; }
        private ChatBot Bot { get; }
        private object Sync { get; } = new object();

        public ChannelManager(
            IEventDispatcher dispatcher,
            IEventTarget target,
            IConfig config,
            ChatBot bot,
            UserManager users
        ) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Bot = bot ?? throw new ArgumentNullException(nameof(bot));
            ChannelNames = Config.ReadCached(@"channels", new[] { @"lounge" });
            UpdateConfigChannels();
        }

        public IChannel DefaultChannel { get; private set; }

        // Needs better name + figure out how to run periodically
        public void UpdateConfigChannels() {
            lock(Sync) {
                string[] channelNames = ChannelNames;

                foreach(IChannel channel in Channels) {
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

        // Should this be here?
        // Should there be a Channel User relationship thing?
        public bool HasUser(IChannel channel, IUser user) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            if(channel is Channel c)
                return c.HasUser(user);

            lock(Sync) {
                return false;
            }
        }

        public void GetUsers(IChannel channel, Action<IEnumerable<IUser>> callable) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(callable == null)
                throw new ArgumentNullException(nameof(callable));

            if(channel is Channel c) {
                c.GetUsers(callable);
                return;
            }

            lock(Sync) {
                return;
            }
        }

        public void Add(IChannel channel) {
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

                // Broadcast creation of channel (deprecated)
                if(Users != null)
                    foreach(IUser user in Users.OfRank(channel.MinimumRank))
                        user.SendPacket(new ChannelCreatePacket(channel));
            }
        }

        public void Remove(IChannel channel, IUser user = null) {
            if(channel == null || channel == DefaultChannel)
                return;

            lock(Sync) {
                // Remove channel from the listing
                Channels.Remove(channel);

                // Broadcast death
                Dispatcher.DispatchEvent(this, new ChannelDeleteEvent(channel, user ?? Bot));

                // Move all users back to the main channel
                // TODO:!!!!!!!!! Replace this with a kick. SCv2 supports being in 0 channels, SCv1 should force the user back to DefaultChannel.
                // Could be handled by the user/session itself?
                //foreach(ChatUser user in channel.GetUsers()) {
                //    Context.SwitchChannel(user, DefaultChannel);
                //}

                // Broadcast deletion of channel (deprecated)
                foreach(IUser u in Users.OfRank(channel.MinimumRank))
                    u.SendPacket(new ChannelDeletePacket(channel));
            }
        }

        public bool Contains(IChannel chan) {
            if(chan == null)
                return false;

            lock(Sync)
                return Channels.Contains(chan)
                    || Channels.Any(c => c.Name.ToLowerInvariant() == chan.Name.ToLowerInvariant());
        }

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync)
                switch(evt) {
                    case ChannelCreateEvent create:
                        //if(sender != this)
                        //    Add();
                        break;

                    case ChannelDeleteEvent delete:
                        //if(sender != this)
                            //Remove();
                        break;

                    case ChannelUpdateEvent _:
                    case ChannelJoinEvent _:
                    case ChannelLeaveEvent _:
                        IChannel chan = Get(evt.Target);
                        if(chan is IEventHandler ceh)
                            ceh.HandleEvent(sender, evt);
                        break;
                }
        }

        public IChannel Create(
            IUser owner,
            string name,
            bool temp = true,
            int minRank = 0,
            string password = null,
            bool autoJoin = false,
            uint maxCapacity = 0
        ) {
            lock(Sync) {
                IChannel channel = new Channel(name, temp, minRank, password, autoJoin, maxCapacity, owner);
                Add(channel);

                Dispatcher.DispatchEvent(this, new ChannelCreateEvent(Target, channel));

                return channel;
            }
        }

        public void Update(IChannel channel, string name = null, bool? temporary = null, int? minRank = null, string password = null, bool? autoJoin = null, uint? maxCapacity = null) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(!Channels.Contains(channel))
                throw new ArgumentException(@"Provided channel is not registered with this manager.", nameof(channel));

            lock(Sync) {
                string prevName = channel.Name;
                bool nameUpdated = !string.IsNullOrWhiteSpace(name) && name != prevName;

                if(nameUpdated) {
                    if(!name.All(c => char.IsLetter(c) || char.IsNumber(c) || c == '-'))
                        throw new ChannelInvalidNameException();
                    if(Get(name) != null)
                        throw new ChannelExistException();
                }

                if(temporary.HasValue && channel.IsTemporary == temporary.Value)
                    temporary = null;

                if(minRank.HasValue && channel.MinimumRank == minRank.Value)
                    minRank = null;

                if(password != null && channel.Password == password)
                    password = null;

                if(autoJoin.HasValue && channel.AutoJoin == autoJoin.Value)
                    autoJoin = null;

                if(maxCapacity.HasValue && channel.MaxCapacity == maxCapacity.Value)
                    maxCapacity = null;

                Dispatcher.DispatchEvent(this, new ChannelUpdateEvent(channel, Bot, name, temporary, minRank, password, autoJoin, maxCapacity));

                // Users that no longer have access to the channel/gained access to the channel by the hierarchy change should receive delete and create packets respectively
                // TODO: should be moved to the usermanager probably
                foreach(IUser user in Users.OfRank(channel.MinimumRank)) {
                    user.SendPacket(new ChannelUpdatePacket(prevName, channel));

                    if(nameUpdated)
                        user.ForceChannel();
                }
            }
        }

        public IChannel Get(string name) {
            if(string.IsNullOrWhiteSpace(name))
                return null;

            lock(Sync) {
                return Channels.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            }
        }

        public IEnumerable<IChannel> OfHierarchy(int hierarchy) {
            lock(Sync) {
                return Channels.Where(c => c.MinimumRank <= hierarchy);
            }
        }
    }
}
