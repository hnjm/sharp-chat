using SharpChat.Events;
using SharpChat.Flashii;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace SharpChat {
    public class ChatContext : IDisposable, IPacketTarget {
        public bool IsDisposed { get; private set; }

        public readonly SockChatServer Server;
        public readonly Timer BumpTimer;
        public readonly BanManager Bans;
        public readonly ChannelManager Channels;
        public readonly UserManager Users;
        public readonly ChatEventManager Events;

        public string TargetName => @"@broadcast";

        public ChatContext(SockChatServer server) {
            Server = server;
            Bans = new BanManager(this);
            Users = new UserManager(this);
            Channels = new ChannelManager(this);
            Events = new ChatEventManager(this);

            BumpTimer = new Timer(e => {
                lock (Users)
                    FlashiiBump.Submit(Users);
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public void Update() {
            Bans.RemoveExpired();
            CheckPings();
        }

        public void BanUser(ChatUser user, DateTimeOffset? until = null, bool banIPs = false, UserDisconnectReason reason = UserDisconnectReason.Kicked) {
            if (until.HasValue && until.Value <= DateTimeOffset.UtcNow)
                until = null;

            if (until.HasValue) {
                user.Send(new ForceDisconnectPacket(ForceDisconnectReason.Banned, until.Value));
                Bans.Add(user, until.Value);

                if (banIPs)
                    lock (user.Connections)
                        foreach (IPAddress ip in user.RemoteAddresses)
                            Bans.Add(ip, until.Value);
            } else
                user.Send(new ForceDisconnectPacket(ForceDisconnectReason.Kicked));

            user.Close();
            UserLeave(user.Channel, user, reason);
        }

        public IEnumerable<IChatEvent> GetChannelBacklog(ChatChannel chan, int count = 20) {
            return Events.Where(x => x.Target == chan || x.Target == null).Reverse().Take(count).Reverse().ToArray();
        }

        public void HandleJoin(ChatUser user, ChatChannel chan, ChatUserConnection conn) {
            if (!chan.HasUser(user))
                lock (Events) {
                    chan.Send(new UserConnectPacket(DateTimeOffset.Now, user));
                    Events.Add(new UserConnectEvent(DateTimeOffset.Now, user, chan));
                }

            conn.Send(new AuthSuccessPacket(user, chan));
            conn.Send(new ContextUsersPacket(chan.GetUsers(new[] { user })));

            IEnumerable<IChatEvent> msgs = GetChannelBacklog(chan);

            foreach (IChatEvent msg in msgs)
                conn.Send(new ContextMessagePacket(msg));

            lock (Channels)
                conn.Send(new ContextChannelsPacket(Channels.Where(x => user.Hierarchy >= x.Hierarchy)));

            if (!chan.HasUser(user))
                chan.UserJoin(user);

            if (!Users.Contains(user))
                Users.Add(user);
        }

        public void UserLeave(ChatChannel chan, ChatUser user, UserDisconnectReason reason = UserDisconnectReason.Leave) {
            user.Status = ChatUserStatus.Offline;

            if (chan == null) {
                Channels.Where(x => x.Users.Contains(user)).ToList().ForEach(x => UserLeave(x, user, reason));
                return;
            }

            if (chan.IsTemporary && chan.Owner == user)
                Channels.Remove(chan);

            chan.UserLeave(user);

            lock (Events) {
                chan.Send(new UserDisconnectPacket(DateTimeOffset.Now, user, reason));
                Events.Add(new UserDisconnectEvent(DateTimeOffset.Now, user, chan, reason));
            }
        }

        public void SwitchChannel(ChatUser user, ChatChannel chan, string password) {
            if (user.Channel == chan) {
                //user.Send(true, @"samechan", chan.Name);
                user.ForceChannel();
                return;
            }

            if (!user.Can(ChatUserPermissions.JoinAnyChannel) && chan.Owner != user) {
                if (chan.Hierarchy > user.Hierarchy) {
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

        public void ForceChannelSwitch(ChatUser user, ChatChannel chan) {
            if (!Channels.Contains(chan))
                return;

            ChatChannel oldChan = user.Channel;

            lock (Events) {
                oldChan.Send(new UserChannelLeavePacket(user));
                Events.Add(new UserChannelLeaveEvent(DateTimeOffset.Now, user, oldChan));
                chan.Send(new UserChannelJoinPacket(user));
                Events.Add(new UserChannelJoinEvent(DateTimeOffset.Now, user, chan));
            }

            user.Send(new ContextClearPacket(ContextClearMode.MessagesUsers));
            user.Send(new ContextUsersPacket(chan.GetUsers(new[] { user })));

            IEnumerable<IChatEvent> msgs = GetChannelBacklog(chan);

            foreach (IChatEvent msg in msgs)
                user.Send(new ContextMessagePacket(msg));

            user.ForceChannel(chan);
            oldChan.UserLeave(user);
            chan.UserJoin(user);

            if (oldChan.IsTemporary && oldChan.Owner == user)
                Channels.Remove(oldChan);
        }

        public void CheckPings() {
            List<ChatUser> users;

            lock (Users)
                users = new List<ChatUser>(Users);

            foreach (ChatUser user in users) {
                List<ChatUserConnection> conns = new List<ChatUserConnection>(user.Connections);

                foreach (ChatUserConnection conn in conns) {
                    if (conn.HasTimedOut) {
                        user.Connections.Remove(conn);
                        conn.Dispose();
                        Logger.Write($@"Nuked a connection from {user.Username}");
                    }

                    if (user.Connections.Count < 1)
                        UserLeave(null, user, UserDisconnectReason.TimeOut);
                }
            }
        }

        public void Send(IServerPacket packet) {
            lock (Users)
                foreach (ChatUser user in Users)
                    user.Send(packet);
        }

        ~ChatContext()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing) {
            if (IsDisposed)
                return;
            IsDisposed = true;

            BumpTimer?.Dispose();
            Events?.Dispose();
            Channels?.Dispose();
            Users?.Dispose();
            Bans?.Dispose();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
