using SharpChat.Bans;
using SharpChat.Channels;
using SharpChat.Configuration;
using SharpChat.Database;
using SharpChat.DataProvider;
using SharpChat.Events;
using SharpChat.Events.Storage;
using SharpChat.Packets;
using SharpChat.RateLimiting;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace SharpChat {
    public class ChatContext : IDisposable, IPacketTarget {
        public BanManager Bans { get; }
        public ChannelManager Channels { get; }
        public UserManager Users { get; }
        public SessionManager Sessions { get; }
        public RateLimiter RateLimiter { get; }

        public DatabaseWrapper Database { get; }

        public IChatEventStorage Events { get; }
        public IDataProvider DataProvider { get; }
        public IConfig Config { get; }

        public ChatBot Bot { get; } = new ChatBot(); 

        private Timer BumpTimer { get; }
        private Timer BansTimer { get; }

        public string TargetName => @"@broadcast";

        public const int DEFAULT_MSG_LENGTH_MAX = 2100;
        private CachedValue<int> MessageTextMaxLengthValue { get; }
        public int MessageTextMaxLength => MessageTextMaxLengthValue;

        public ChatContext(IConfig config, IDatabaseBackend databaseBackend, IDataProvider dataProvider) {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Database = new DatabaseWrapper(databaseBackend ?? throw new ArgumentNullException(nameof(databaseBackend)));
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Bans = new BanManager(this);
            Users = new UserManager(this);
            Channels = new ChannelManager(this, config);
            Sessions = new SessionManager(config.ScopeTo(@"sessions"));
            RateLimiter = new RateLimiter(Config.ScopeTo(@"flood"));
            Events = Database.IsNullBackend
                ? new MemoryChatEventStorage()
                : new ADOChatEventStorage(Database);

            MessageTextMaxLengthValue = Config.ReadCached(@"messages:maxLength", DEFAULT_MSG_LENGTH_MAX);

            // Should probably not rely on Timers in the future
            BumpTimer = new Timer(e => {
                Logger.Write(@"Bumping last online times...");
                DataProvider.UserBumpClient.SubmitBumpUsers(Users.WithActiveConnections());
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            BansTimer = new Timer(e => {
                Bans.RefreshRemoteBans();
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
        }

        public void Update() { // this should probably not exist, or at least not called the way it is
            Sessions.DisposeTimedOut();
            Bans.RemoveExpired();
            PruneSessionlessUsers(); // this function also needs to go
        }

        public void BanUser(ChatUser user, DateTimeOffset? until = null, bool banIPs = false, UserDisconnectReason reason = UserDisconnectReason.Kicked) {
            if (until.HasValue && until.Value <= DateTimeOffset.Now)
                until = null;

            if (until.HasValue) {
                user.Send(new ForceDisconnectPacket(ForceDisconnectReason.Banned, until.Value));
                Bans.Add(user, until.Value);

                if (banIPs) {
                    foreach (IPAddress ip in user.RemoteAddresses)
                        Bans.Add(ip, until.Value);
                }
            } else
                user.Send(new ForceDisconnectPacket(ForceDisconnectReason.Kicked));

            user.Close();
            UserLeave(user.Channel, user, reason);
        }

        public void UserLeave(Channel chan, ChatUser user, UserDisconnectReason reason = UserDisconnectReason.Leave) {
            user.Status = UserStatus.Offline;

            if (chan == null) {
                foreach(Channel channel in user.GetChannels()) {
                    UserLeave(channel, user, reason);
                }
                return;
            }

            if (chan.IsTemporary && chan.Owner == user)
                Channels.Remove(chan);

            chan.UserLeave(user);
            chan.Send(new UserDisconnectPacket(DateTimeOffset.Now, user, reason));
            Events.AddEvent(new UserDisconnectEvent(DateTimeOffset.Now, user, chan, reason));
        }

        public void SwitchChannel(ChatUser user, Channel chan, string password) {
            if (user.CurrentChannel == chan) {
                //user.Send(true, @"samechan", chan.Name);
                user.ForceChannel();
                return;
            }

            if (!user.Can(UserPermissions.JoinAnyChannel) && chan.Owner != user) {
                if (chan.MinimumRank > user.Rank) {
                    user.Send(new LegacyCommandResponse(LCR.CHANNEL_INSUFFICIENT_HIERARCHY, true, chan.Name));
                    user.ForceChannel();
                    return;
                }

                if (chan.Password != password) {
                    user.Send(new LegacyCommandResponse(LCR.CHANNEL_INVALID_PASSWORD, true, chan.Name));
                    user.ForceChannel();
                    return;
                }
            }

            ForceChannelSwitch(user, chan);
        }

        public void ForceChannelSwitch(ChatUser user, Channel chan) {
            if (!Channels.Contains(chan))
                return;

            Channel oldChan = user.CurrentChannel;

            oldChan.Send(new UserChannelLeavePacket(user));
            Events.AddEvent(new UserChannelLeaveEvent(DateTimeOffset.Now, user, oldChan));
            chan.Send(new UserChannelJoinPacket(user));
            Events.AddEvent(new UserChannelJoinEvent(DateTimeOffset.Now, user, chan));

            user.Send(new ContextClearPacket(chan, ContextClearMode.MessagesUsers));
            user.Send(new ContextUsersPacket(chan.GetUsers(new[] { user })));

            IEnumerable<IEvent> msgs = Events.GetEventsForTarget(chan);

            foreach (IEvent msg in msgs)
                user.Send(new ContextMessagePacket(msg));

            user.ForceChannel(chan);
            oldChan.UserLeave(user);
            chan.UserJoin(user);

            if (oldChan.IsTemporary && oldChan.Owner == user)
                Channels.Remove(oldChan);
        }

        public void PruneSessionlessUsers() {
            lock(Users)
                foreach (ChatUser user in Users.All())
                    if(Sessions.GetSessionCount(user) < 1)
                        UserLeave(null, user, UserDisconnectReason.TimeOut);
        }

        public void Send(IServerPacket packet) {
            foreach (ChatUser user in Users.All())
                user.Send(packet);
        }

        private bool IsDisposed;
        ~ChatContext()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if (IsDisposed)
                return;
            IsDisposed = true;

            BansTimer.Dispose();
            BumpTimer.Dispose();

            Sessions.Dispose();
            Events.Dispose();
            Channels.Dispose();
            Users.Dispose();
            Bans.Dispose();
        }
    }
}
