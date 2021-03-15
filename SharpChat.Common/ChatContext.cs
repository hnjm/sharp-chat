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
using System.Threading;

namespace SharpChat {
    public class ChatContext : IDisposable, IEventHandler, IServerPacketTarget {
        public ChannelManager Channels { get; }
        public UserManager Users { get; }
        public SessionManager Sessions { get; }
        public RateLimiter RateLimiter { get; }

        public IChatEventStorage Events { get; }
        public IDataProvider DataProvider { get; }

        public ChatBot Bot { get; } = new ChatBot(); 

        private Timer BumpTimer { get; }

        public const int DEFAULT_MSG_LENGTH_MAX = 2100;
        private CachedValue<int> MessageTextMaxLengthValue { get; }
        public int MessageTextMaxLength => MessageTextMaxLengthValue;

        public ChatContext(IConfig config, IDatabaseBackend databaseBackend, IDataProvider dataProvider) {
            if(config == null)
                throw new ArgumentNullException(nameof(config));

            DatabaseWrapper db = new DatabaseWrapper(databaseBackend ?? throw new ArgumentNullException(nameof(databaseBackend)));
            Events = db.IsNullBackend
                ? new MemoryChatEventStorage()
                : new ADOChatEventStorage(db);
            Event.RegisterConstructors(Events);

            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Users = new UserManager(this);
            Channels = new ChannelManager(this, config);
            Sessions = new SessionManager(config.ScopeTo(@"sessions"));
            RateLimiter = new RateLimiter(config.ScopeTo(@"flood"));

            MessageTextMaxLengthValue = config.ReadCached(@"messages:maxLength", DEFAULT_MSG_LENGTH_MAX);

            // Should probably not rely on Timers in the future
            BumpTimer = new Timer(e => {
                Logger.Write(@"Bumping last online times...");
                DataProvider.UserBumpClient.SubmitBumpUsers(Users.WithActiveConnections());
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public void Update() { // this should probably not exist, or at least not called the way it is
            Sessions.DisposeTimedOut();
            PruneSessionlessUsers(); // this function also needs to go
        }

        public void BanUser(
            ChatUser user,
            TimeSpan duration,
            UserDisconnectReason reason = UserDisconnectReason.Kicked,
            bool isPermanent = false,
            IUser modUser = null,
            string textReason = null
        ) {
            ForceDisconnectPacket packet;

            if(duration != TimeSpan.Zero || isPermanent) {
                if(string.IsNullOrWhiteSpace(textReason))
                    textReason = reason switch {
                        UserDisconnectReason.Kicked => @"User was banned.",
                        UserDisconnectReason.Flood => @"User was kicked for flood protection.",
                        _ => @"Unknown reason given.",
                    };

                DataProvider.BanClient.CreateBan(user.UserId, modUser?.UserId ?? -1, isPermanent, duration, textReason);

                packet = new ForceDisconnectPacket(ForceDisconnectReason.Banned, duration, isPermanent);
            } else
                packet = new ForceDisconnectPacket(ForceDisconnectReason.Kicked);

            user.SendPacket(packet);
            user.Close();

            foreach(Channel chan in user.GetChannels())
                UserLeave(chan, user, reason);
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
            chan.SendPacket(new UserDisconnectPacket(DateTimeOffset.Now, user, reason));
            HandleEvent(new UserDisconnectEvent(DateTimeOffset.Now, user, chan, reason));
        }

        public void JoinChannel(ChatUser user, Channel channel) {
            // These two should be combined into just an event broadcast
            channel.SendPacket(new UserChannelJoinPacket(user));
            HandleEvent(new UserChannelJoinEvent(DateTimeOffset.Now, user, channel));

            user.SendPacket(new ContextClearPacket(channel, ContextClearMode.MessagesUsers));
            user.SendPacket(new ContextUsersPacket(channel.GetUsers(new[] { user })));
            IEnumerable<IEvent> msgs = Events.GetEventsForTarget(channel);
            foreach(IEvent msg in msgs)
                user.SendPacket(new ContextMessagePacket(msg));

            channel.UserJoin(user);
            user.ForceChannel(channel);
        }

        public void LeaveChannel(ChatUser user, Channel channel) {
            channel.UserLeave(user);

            // These two should be combined into just an event broadcast
            channel.SendPacket(new UserChannelLeavePacket(user));
            HandleEvent(new UserChannelLeaveEvent(DateTimeOffset.Now, user, channel));

            if(channel.IsTemporary && channel.Owner == user)
                Channels.Remove(channel);
        }

        public void SwitchChannel(Session session, Channel chan) {
            if(session.LastChannel != null)
                LeaveChannel(session.User, session.LastChannel);
            JoinChannel(session.User, chan);
        }

        public void PruneSessionlessUsers() {
            lock(Users)
                foreach (ChatUser user in Users.All())
                    if(Sessions.GetSessionCount(user) < 1)
                        UserLeave(null, user, UserDisconnectReason.TimeOut);
        }

        public void SendPacket(IServerPacket packet) {
            foreach (ChatUser user in Users.All())
                user.SendPacket(packet);
        }

        public void HandleEvent(IEvent evt) {
            Events.HandleEvent(evt);
            Channels.HandleEvent(evt);
            Users.HandleEvent(evt);
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

            BumpTimer.Dispose();
        }
    }
}
