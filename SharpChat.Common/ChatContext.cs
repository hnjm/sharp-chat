using SharpChat.Channels;
using SharpChat.Configuration;
using SharpChat.Database;
using SharpChat.DataProvider;
using SharpChat.Events;
using SharpChat.Events.Storage;
using SharpChat.Messages;
using SharpChat.Packets;
using SharpChat.RateLimiting;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace SharpChat {
    public class ChatContext : IDisposable, IEventDispatcher, IEventTarget, IServerPacketTarget {
        public ChannelManager Channels { get; }
        public MessageManager Messages { get; }
        public UserManager Users { get; }
        public SessionManager Sessions { get; }
        public RateLimiter RateLimiter { get; }

        public IEventStorage Events { get; }
        public IDataProvider DataProvider { get; }

        public ChatBot Bot { get; } = new ChatBot(); 

        private Timer BumpTimer { get; }
        private readonly object Sync = new object();

        public string TargetName => @"~";

        public ChatContext(Guid serverId, IConfig config, IDatabaseBackend databaseBackend, IDataProvider dataProvider) {
            if(config == null)
                throw new ArgumentNullException(nameof(config));

            DatabaseWrapper db = new DatabaseWrapper(databaseBackend ?? throw new ArgumentNullException(nameof(databaseBackend)));
            Events = db.IsNullBackend
                ? new MemoryEventStorage()
                : new ADOEventStorage(db);
            Event.RegisterConstructors(Events);

            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Sessions = new SessionManager(serverId, config.ScopeTo(@"sessions"));
            Users = new UserManager(this, this, Sessions);
            Channels = new ChannelManager(this, this, config, Bot, Users);
            Messages = new MessageManager(this, this, Events, config.ScopeTo(@"messages"));
            RateLimiter = new RateLimiter(config.ScopeTo(@"flood"));

            // Should probably not rely on Timers in the future
            BumpTimer = new Timer(e => {
                Logger.Write(@"Bumping last online times...");
                DataProvider.UserBumpClient.SubmitBumpUsers(Sessions, Users.WithActiveConnections());
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public void Update() { // this should probably not exist, or at least not called the way it is
            Sessions.DisposeTimedOut();
            PruneSessionlessUsers(); // this function also needs to go
        }

        public void BanUser(
            IUser user,
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

            foreach(IChannel chan in user.GetChannels())
                UserLeave(chan, user, reason);
        }

        public void UserLeave(IChannel chan, IUser user, UserDisconnectReason reason = UserDisconnectReason.Leave) {
            if (chan == null) {
                foreach(IChannel channel in user.GetChannels()) {
                    UserLeave(channel, user, reason);
                }
                return;
            }

            if (chan.IsTemporary && chan.Owner == user)
                Channels.Remove(chan);

            chan.UserLeave(user);
            chan.SendPacket(new UserDisconnectPacket(DateTimeOffset.Now, user, reason));
            Users.Disconnect(user, reason);
        }

        public void JoinChannel(IUser user, IChannel channel) {
            // These two should be combined into just an event broadcast
            channel.SendPacket(new UserChannelJoinPacket(user));
            DispatchEvent(this, new ChannelJoinEvent(channel, user));

            user.SendPacket(new ContextClearPacket(channel, ContextClearMode.MessagesUsers));
            channel.GetUsers(users => user.SendPacket(new ContextUsersPacket(users.Except(new[] { user }).OrderByDescending(u => u.Rank))));
            IEnumerable<IEvent> msgs = Events.GetEventsForTarget(channel);
            foreach(IEvent msg in msgs)
                user.SendPacket(new ContextMessagePacket(msg));

            channel.UserJoin(user);
            user.ForceChannel(channel);
        }

        public void LeaveChannel(IUser user, IChannel channel) {
            channel.UserLeave(user);

            // These two should be combined into just an event broadcast
            channel.SendPacket(new UserChannelLeavePacket(user));
            DispatchEvent(this, new ChannelLeaveEvent(channel, user));

            if(channel.IsTemporary && channel.Owner == user)
                Channels.Remove(channel);
        }

        public void SwitchChannel(Session session, IChannel chan) {
            if(session.LastChannel != null)
                LeaveChannel(session.User, session.LastChannel);
            JoinChannel(session.User, chan);
        }

        public void PruneSessionlessUsers() {
            lock(Users)
                foreach(IUser user in Users.All())
                    if(Sessions.GetSessionCount(user) < 1)
                        UserLeave(null, user, UserDisconnectReason.TimeOut);
        }

        // Obsolete?
        public void SendPacket(IServerPacket packet) {
            foreach(IUser user in Users.All())
                if(user is IServerPacketTarget spt)
                    spt.SendPacket(packet);
        }

        public void DispatchEvent(object sender, IEvent evt) {
            if(evt == null)
                throw new ArgumentNullException(nameof(evt));

            lock(Sync) {
                Logger.Debug($@"{evt.GetType()} dispatched.");

                Events.HandleEvent(sender, evt);
                Messages.HandleEvent(sender, evt);
                Channels.HandleEvent(sender, evt);
                Users.HandleEvent(sender, evt);
                Sessions.HandleEvent(sender, evt);
            }
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
