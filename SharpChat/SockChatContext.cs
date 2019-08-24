using Fleck;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace SharpChat
{
    public class SockChatContext : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public readonly SockChatServer Server;
        public readonly List<SockChatUser> Users = new List<SockChatUser>();
        public readonly List<SockChatChannel> Channels = new List<SockChatChannel>();
        public readonly List<IChatMessage> Messages = new List<IChatMessage>();
        public readonly Dictionary<IPAddress, DateTimeOffset> IPBans = new Dictionary<IPAddress, DateTimeOffset>();
        public readonly Timer BumpTimer;

        public SockChatContext(SockChatServer server)
        {
            Server = server;
            BumpTimer = new Timer(e => BumpFlashiiOnline(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public void CheckIPBanExpirations()
        {
            lock(IPBans)
                IPBans.Where(kvp => kvp.Value < DateTimeOffset.UtcNow).Select(kvp => kvp.Key).ToList().ForEach(ip => IPBans.Remove(ip));
        }

        public bool CheckIPBan(string ipAddr)
        {
            return GetIPBanExpiration(ipAddr) > DateTimeOffset.UtcNow;
        }

        public bool CheckIPBan(IPAddress ipAddr)
        {
            return GetIPBanExpiration(ipAddr) > DateTimeOffset.UtcNow;
        }

        public DateTimeOffset GetIPBanExpiration(string ipAddr)
        {
            if (IPAddress.TryParse(ipAddr, out IPAddress ip))
                return GetIPBanExpiration(ip);
            return DateTimeOffset.MinValue;
        }

        public DateTimeOffset GetIPBanExpiration(IPAddress ipAddr)
        {
            lock(IPBans)
            {
                if (!IPBans.ContainsKey(ipAddr))
                    return DateTimeOffset.MinValue;
                return IPBans[ipAddr];
            }
        }

        public void BanUser(SockChatUser user, DateTimeOffset? until = null, bool banIPs = false, string type = Constants.LEAVE_KICK)
        {
            if (until.HasValue && until.Value <= DateTimeOffset.UtcNow)
                until = null;

            if (until.HasValue)
            {
                user.Send(new ForceDisconnectPacket(ForceDisconnectReason.Banned, until.Value));
                user.BannedUntil = until.Value;

                if (banIPs)
                    lock (user.Connections)
                        foreach (string ip in user.RemoteAddresses)
                            if (IPAddress.TryParse(ip, out IPAddress ipAddr))
                                IPBans[ipAddr] = until.Value;
            }
            else
                user.Send(new ForceDisconnectPacket(ForceDisconnectReason.Kicked));

            user.Close();
            UserLeave(user.Channel, user, type);
        }

        public SockChatChannel DefaultChannel
            => Channels.First();

        public string AddChannel(SockChatChannel chan)
        {
            lock (Channels)
            {
                SockChatChannel eChan = FindChannelByName(chan.Name);
                if (eChan != null)
                    return SockChatMessage.PackBotMessage(1, @"nischan", chan.Name);

                if (chan.Name.StartsWith(@"@") || chan.Name.StartsWith(@"*"))
                    return SockChatMessage.PackBotMessage(1, @"inchan");

                Channels.Add(chan);

                lock (Users)
                    Users.Where(u => u.Hierarchy >= chan.Hierarchy).ForEach(u => u.Send(new ChannelCreatePacket(chan)));
            }

            return null;
        }

        public void DeleteChannel(SockChatChannel chan)
        {
            if (chan == DefaultChannel)
                return;

            lock (chan.Users)
                lock (Users)
                    lock (Channels)
                    {
                        chan.Users.ForEach(u => SwitchChannel(u, DefaultChannel, string.Empty));
                        Users.Where(u => u.Hierarchy >= chan.Hierarchy).ForEach(u => u.Send(new ChannelDeletePacket(chan)));
                        Channels.Remove(chan);
                    }
        }

        public void UpdateChannel(SockChatChannel chan, string oldName = null)
        {
            lock (Users)
                Users.Where(u => u.Hierarchy >= chan.Hierarchy).ForEach(u =>
                {
                    u.Send(new ChannelUpdatePacket(oldName ?? chan.Name, chan));

                    /* Not entire sure how to recreate this behaviour at the moment
                    if ($user->channel == $oldname && $oldname != "") {
                        $user->runSock(function($sock) use ($channel) {
                            $sock->send(pack_message(5, ["2", $channel->name]));
                        });
                        $user->channel = $channel->name;
                    }
                     */
                    u.ForceChannel();
                });
        }

        public void DeleteMessage(IChatMessage msg)
        {
            lock (Messages)
                Messages.Remove(msg);

            Broadcast(new ChatMessageDeletePacket(msg.MessageId));
        }

        public SockChatUser FindUserById(int userId)
        {
            lock (Users)
                return Users.ToList().FirstOrDefault(x => x.UserId == userId);
        }
        public SockChatUser FindUserByName(string name)
        {
            lock (Users)
                return Users.FirstOrDefault(x => x.Username.ToLowerInvariant() == name.ToLowerInvariant() || x.DisplayName.ToLowerInvariant() == name.ToLowerInvariant());
        }

        public SockChatUser FindUserBySock(SockChatConn conn)
        {
            lock (Users)
                return Users.ToList().FirstOrDefault(x => x.Connections.Any(y => y == conn));
        }

        public SockChatUser FindUserBySock(IWebSocketConnection conn)
        {
            lock (Users)
                return Users.ToList().FirstOrDefault(x => x.Connections.Any(y => y.Websocket == conn));
        }

        public SockChatChannel FindChannelByName(string name)
        {
            return Channels.FirstOrDefault(x => x.Name.ToLowerInvariant().Trim() == name.ToLowerInvariant().Trim());
        }

        public SockChatChannel FindUserChannel(SockChatUser user)
        {
            return Channels.FirstOrDefault(c => c.Users.Contains(user));
        }

        public IChatMessage[] GetChannelBacklog(SockChatChannel chan, int count = 15)
        {
            return Messages.Where(x => x.Channel == chan || x.Channel == null).Reverse().Take(count).Reverse().ToArray();
        }

        public void HandleJoin(SockChatUser user, SockChatChannel chan, SockChatConn conn)
        {
            if (!chan.HasUser(user))
                chan.Send(new UserConnectPacket(DateTimeOffset.Now, user), SockChatMessage.NextMessageId);

            conn.Send(new AuthSuccessPacket(user, chan));
            conn.Send(new ContextUsersPacket(chan.GetUsers(new[] { user })));

            IChatMessage[] msgs = GetChannelBacklog(chan);

            foreach (IChatMessage msg in msgs)
                conn.Send(new ContextMessagePacket(msg));

            lock (Channels)
                conn.Send(new ContextChannelsPacket(Channels.Where(x => user.Hierarchy >= x.Hierarchy)));

            if (!chan.HasUser(user))
                chan.UserJoin(user);

            if (!Users.Contains(user))
                Users.Add(user);
        }

        public void UserLeave(SockChatChannel chan, SockChatUser user, string type = Constants.LEAVE_NORMAL)
        {
            if (chan == null)
            {
                Channels.Where(x => x.Users.Contains(user)).ToList().ForEach(x => UserLeave(x, user, type));
                return;
            }

            if (chan.IsTemporary && chan.Owner == user)
                DeleteChannel(chan);

            chan.UserLeave(user);
            HandleLeave(chan, user, type);
        }

        public void HandleLeave(SockChatChannel chan, SockChatUser user, string type = Constants.LEAVE_NORMAL)
        {
            UserDisconnectReason reason = UserDisconnectReason.Leave;

            switch (type)
            {
                case Constants.LEAVE_TIMEOUT:
                    reason = UserDisconnectReason.TimeOut;
                    break;
                case Constants.LEAVE_KICK:
                    reason = UserDisconnectReason.Kicked;
                    break;
                case Constants.LEAVE_FLOOD:
                    reason = UserDisconnectReason.Flood;
                    break;
            }

            chan.Send(new UserDisconnectPacket(DateTimeOffset.Now, user, reason));
        }

        public void SwitchChannel(SockChatUser user, string chanName, string password)
        {
            SockChatChannel chan = FindChannelByName(chanName);

            if (chan == null)
            {
                user.Send(true, @"nochan", chanName);
                user.ForceChannel();
                return;
            }

            SwitchChannel(user, chan, password);
        }

        public void SwitchChannel(SockChatUser user, SockChatChannel chan, string password)
        {
            if (user.Channel == chan)
            {
                //user.Send(true, @"samechan", chan.Name);
                user.ForceChannel();
                return;
            }

            if (!user.IsModerator && chan.Owner != user)
            {
                if (chan.Hierarchy > user.Hierarchy)
                {
                    user.Send(true, @"ipchan", chan.Name);
                    user.ForceChannel();
                    return;
                }

                if (chan.Password != password)
                {
                    user.Send(true, @"ipwchan", chan.Name);
                    user.ForceChannel();
                    return;
                }
            }

            ForceChannelSwitch(user, chan);
        }

        public void ForceChannelSwitch(SockChatUser user, SockChatChannel chan)
        {
            if (!Channels.Contains(chan))
                return;

            int messageId = SockChatMessage.NextMessageId;
            SockChatChannel oldChan = user.Channel;

            oldChan.Send(new UserChannelLeavePacket(user), messageId);
            chan.Send(new UserChannelJoinPacket(user), messageId);

            user.Send(new ContextClearPacket(ContextClearMode.MessagesUsers));
            user.Send(new ContextUsersPacket(chan.GetUsers(new[] { user })));

            IChatMessage[] msgs = GetChannelBacklog(chan);

            foreach (IChatMessage msg in msgs)
                user.Send(new ContextMessagePacket(msg));

            user.ForceChannel(chan);
            oldChan.UserLeave(user);
            chan.UserJoin(user);

            if(oldChan.IsTemporary && oldChan.Owner == user)
                DeleteChannel(oldChan);
        }

        public void CheckPings()
        {
            List<SockChatUser> users;

            lock(Users)
                users = new List<SockChatUser>(Users);

            foreach (SockChatUser user in users)
            {
                List<SockChatConn> conns = new List<SockChatConn>(user.Connections);

                foreach (SockChatConn conn in conns)
                {
                    if (conn.HasTimedOut)
                    {
                        user.Connections.Remove(conn);
                        conn.Dispose();
                        Logger.Write($@"Nuked a connection from {user.Username} {conn.HasTimedOut} {conn.Websocket.IsAvailable}");
                    }

                    if (user.Connections.Count < 1)
                        UserLeave(null, user, Constants.LEAVE_TIMEOUT);
                }
            }
        }

        public void BumpFlashiiOnline()
        {
            List<FlashiiBump> bups = new List<FlashiiBump>();

            lock (Users)
                Users.Where(u => u.IsAlive).ForEach(u => bups.Add(new FlashiiBump { UserId = u.UserId, UserIP = u.RemoteAddresses.First() }));

            if(bups.Any())
                FlashiiBump.Submit(bups);
        }

        public void Broadcast(IServerPacket packet, int eventId = 0)
        {
            lock (Users)
                Users.ForEach(u => u.Send(packet, eventId));
        }

        [Obsolete(@"Use Broadcast(IServerPacket, int)")]
        public void Broadcast(SockChatUser user, string message, MessageFlags flags = MessageFlags.RegularUser)
        {
            lock (Users)
                Users.ForEach(u => u.Send(user, message, flags));
        }

        ~SockChatContext()
        => Dispose(false);

        public void Dispose()
        => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            IsDisposed = true;

            BumpTimer?.Dispose();
            Messages.Clear();
            Channels.Clear();
            Users.Clear();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
