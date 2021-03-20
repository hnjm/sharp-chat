using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Channels {
    public class Channel : IChannel, IEventHandler {
        public string Name { get; private set; }
        public bool IsTemporary { get; private set; }
        public int MinimumRank { get; private set; }
        public bool AutoJoin { get; private set; }
        public uint MaxCapacity { get; private set; }
        public IUser Owner { get; private set; }

        private readonly object Sync = new object();
        private List<IUser> Users { get; } = new List<IUser>();

        public string Password { get; private set; } = string.Empty;
        public bool HasPassword
            => !string.IsNullOrWhiteSpace(Password);

        public bool HasMaxCapacity
            => MaxCapacity > 0;

        public string TargetName { get; private set; }

        public Channel(
            string name,
            bool temp = false,
            int minimumRank = 0,
            string password = null,
            bool autoJoin = false,
            uint maxCapacity = 0,
            IUser owner = null
        ) {
            Name = name;
            TargetName = Name.ToLowerInvariant();
            IsTemporary = temp;
            MinimumRank = minimumRank;
            Password = password ?? string.Empty;
            AutoJoin = autoJoin;
            MaxCapacity = maxCapacity;
            Owner = owner;
        }

        public bool VerifyPassword(string password) {
            if(password == null)
                throw new ArgumentNullException(nameof(password));
            lock(Sync)
                return !HasPassword || Password.Equals(password);
        }

        public bool HasUser(IUser user) {
            if(user == null)
                return false;
            lock(Sync)
                return Users.Any(u => u.Equals(user));
        }

        public void GetUsers(Action<IEnumerable<IUser>> callable) {
            if(callable == null)
                throw new ArgumentNullException(nameof(callable));
            lock(Sync)
                callable(Users);
        }

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync)
                switch(evt) {
                    case ChannelUpdateEvent update:
                        if(update.HasName) {
                            Name = update.Name;
                            TargetName = Name.ToLowerInvariant();
                        }
                        if(update.IsTemporary.HasValue)
                            IsTemporary = update.IsTemporary.Value;
                        if(update.MinimumRank.HasValue)
                            MinimumRank = update.MinimumRank.Value;
                        if(update.HasPassword)
                            Password = update.Password;
                        break;

                    case ChannelJoinEvent join:
                        Users.Add(join.Sender);
                        break;

                    case ChannelLeaveEvent leave:
                        Users.Remove(leave.Sender);
                        break;
                }
        }

        public bool Equals(IChannel other)
            => other != null && Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);
    }
}
