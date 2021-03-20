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
using SharpChat.Messages.Storage;

namespace SharpChat {
    public class ChatContext : IDisposable, IEventDispatcher, IEventTarget, IServerPacketTarget {
        public ChannelManager Channels { get; }
        public MessageManager Messages { get; }
        public UserManager Users { get; }
        public SessionManager Sessions { get; }
        public ChannelUserRelations ChannelUsers { get; }
        public RateLimiter RateLimiter { get; }

        public IDataProvider DataProvider { get; }

        public ChatBot Bot { get; } = new ChatBot(); 

        private Timer BumpTimer { get; }
        private readonly object Sync = new object();

        public string TargetName => @"~";

        private ADOEventStorage Events { get; }

        public ChatContext(Guid serverId, IConfig config, IDatabaseBackend databaseBackend, IDataProvider dataProvider) {
            if(config == null)
                throw new ArgumentNullException(nameof(config));

            DatabaseWrapper db = new DatabaseWrapper(databaseBackend ?? throw new ArgumentNullException(nameof(databaseBackend)));

            if(!db.IsNullBackend) // only leaving to watch things fill in the database for now
                Events = new ADOEventStorage(db);

            IMessageStorage msgStore = db.IsNullBackend
                ? new MemoryMessageStorage()
                : new ADOMessageStorage(db);

            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Sessions = new SessionManager(serverId, config.ScopeTo(@"sessions"));
            Users = new UserManager(this, this);
            Channels = new ChannelManager(this, this, config, Bot);
            ChannelUsers = new ChannelUserRelations(this, Channels, Users);
            Messages = new MessageManager(this, msgStore, config.ScopeTo(@"messages"));
            RateLimiter = new RateLimiter(config.ScopeTo(@"flood"));

            // Should probably not rely on Timers in the future
            BumpTimer = new Timer(e => {
                Logger.Write(@"Bumping last online times...");
                IEnumerable<IUser> users = null;
                Sessions.GetActiveSessions(s => users = s.Select(s => s.User));
                DataProvider.UserBumpClient.SubmitBumpUsers(Sessions, users);
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

            ChannelUsers.GetChannels(user, c => {
                foreach(IChannel chan in c)
                    ChannelUsers.LeaveChannel(chan, user, reason);
            });

            Users.Disconnect(user, reason);
        }

        [Obsolete(@"Refactor to use ChannelUsers.LeaveChannel and Users.Disconnect")]
        public void UserLeave(IChannel chan, IUser user, UserDisconnectReason reason = UserDisconnectReason.Leave) {
            // where do you go
            chan.SendPacket(new UserDisconnectPacket(DateTimeOffset.Now, user, reason));
        }

        [Obsolete(@"Use ChannelUsers.JoinChannel")]
        public void JoinChannel(IUser user, IChannel channel) {
            // this still needs to go somewhere
            channel.SendPacket(new UserChannelJoinPacket(user));

            // what about context clearing?
            user.SendPacket(new ContextClearPacket(channel, ContextClearMode.MessagesUsers));

            // should users grab the user list themselves?
            ChannelUsers.GetUsers(channel, u => user.SendPacket(new ContextUsersPacket(u.Except(new[] { user }).OrderByDescending(u => u.Rank))));

            // what about dispatching the message history?
            Messages.GetMessages(channel, m => {
                foreach(IMessage msg in m)
                    user.SendPacket(new ContextMessagePacket(msg));
            });

            // oh god and then this
            user.ForceChannel(channel);
        }

        [Obsolete(@"Use ChannelUsers.LeaveChannel")]
        public void LeaveChannel(IUser user, IChannel channel) {
            // this still needs to go somewhere
            channel.SendPacket(new UserChannelLeavePacket(user));
        }

        [Obsolete(@"No.")]
        public void SwitchChannel(Session session, IChannel chan) {
            if(session.LastChannel != null)
                ChannelUsers.LeaveChannel(session.LastChannel, session.User, UserDisconnectReason.Leave);
            ChannelUsers.JoinChannel(chan, session.User);
        }

        public void PruneSessionlessUsers() {
            Users.GetUsers(users => {
                foreach(IUser user in users)
                    if(Sessions.GetSessionCount(user) < 1)
                        Users.Disconnect(user, UserDisconnectReason.TimeOut);
            });
        }

        // Obsolete?
        public void SendPacket(IServerPacket packet) {
            Users.GetUsers(users => {
                foreach(IUser user in users)
                    if(user is IServerPacketTarget spt)
                        spt.SendPacket(packet);
            });
        }

        public void DispatchEvent(object sender, IEvent evt) {
            if(evt == null)
                throw new ArgumentNullException(nameof(evt));

            lock(Sync) {
                Logger.Debug($@"{evt.GetType()} dispatched.");

                Events?.HandleEvent(sender, evt);
                Sessions.HandleEvent(sender, evt);
                ChannelUsers.HandleEvent(sender, evt);
                Channels.HandleEvent(sender, evt);
                Users.HandleEvent(sender, evt);
                Messages.HandleEvent(sender, evt);
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
