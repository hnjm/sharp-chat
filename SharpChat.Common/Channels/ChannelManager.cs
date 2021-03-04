﻿using SharpChat.Configuration;
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
        private object Sync { get; } = new object();

        public ChannelManager(ChatContext context, IConfig config) {
            Context = context ?? throw new ArgumentNullException(nameof(config));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            ChannelNames = Config.ReadCached(@"channels", new[] { @"lounge" });
            UpdateConfigChannels();
        }

        public Channel DefaultChannel { get; private set; }

        // Needs better name + figure out how to run periodically
        public void UpdateConfigChannels() {
            lock(Sync) {
                string[] channelNames = ChannelNames;

                foreach(Channel channel in Channels) {
                    if(channelNames.Contains(channel.Name)) {
                        UpdateConfigChannel(channel);
                    } else {
                        // Not in config == temporary
                        channel.IsTemporary = true;
                    }
                }

                foreach(string channelName in channelNames) {
                    if(Channels.Any(x => x.Name == channelName))
                        continue;
                    Channel channel = new Channel(channelName);
                    UpdateConfigChannel(channel);
                    Add(channel);
                }

                if(DefaultChannel == null || DefaultChannel.IsTemporary || !channelNames.Contains(DefaultChannel.Name))
                    DefaultChannel = Channels.FirstOrDefault(c => !c.IsTemporary && c.AutoJoin);
            }
        }

        private void UpdateConfigChannel(Channel channel) {
            IConfig config = Config.ScopeTo($@"channels:{channel.Name}");

            // If we're here, the channel is listed in the config which implicitly means it's not temporary
            channel.IsTemporary = false;

            channel.AutoJoin = config.ReadValue(@"autoJoin", DefaultChannel == null || DefaultChannel == channel);
            if(channel.AutoJoin) {
                if(DefaultChannel == null)
                    DefaultChannel = channel;
            } else { // autojoin channels cannot have a minimum rank or password
                string password = config.ReadValue(@"password", string.Empty);
                if(!string.IsNullOrWhiteSpace(password))
                    channel.Password = password;

                channel.MinimumRank = config.SafeReadValue(@"minRank", 0);
                channel.MaxCapacity = config.SafeReadValue(@"maxCapacity", 0u);
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
                foreach(ChatUser user in Context.Users.OfRank(channel.MinimumRank))
                    user.SendPacket(new ChannelCreatePacket(channel));
            }
        }

        public void Remove(Channel channel) {
            if(channel == null || channel == DefaultChannel)
                return;

            lock(Sync) {
                // Remove channel from the listing
                Channels.Remove(channel);

                // Move all users back to the main channel
                // TODO:!!!!!!!!! Replace this with a kick. SCv2 supports being in 0 channels, SCv1 should force the user back to DefaultChannel.
                //foreach(ChatUser user in channel.GetUsers()) {
                //    Context.SwitchChannel(user, DefaultChannel);
                //}

                // Broadcast deletion of channel
                foreach(ChatUser user in Context.Users.OfRank(channel.MinimumRank))
                    user.SendPacket(new ChannelDeletePacket(channel));
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
            //
        }

        // Should be replaced by an event
        public void Update(Channel channel, string name = null, bool? temporary = null, int? rank = null, string password = null) {
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

                    channel.Name = name;
                }

                if(temporary.HasValue)
                    channel.IsTemporary = temporary.Value;

                if(rank.HasValue)
                    channel.MinimumRank = rank.Value;

                if(password != null)
                    channel.Password = password;

                // Users that no longer have access to the channel/gained access to the channel by the hierarchy change should receive delete and create packets respectively
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
