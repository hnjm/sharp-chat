using SharpChat.Flashii;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace SharpChat
{
    public class ChatContext : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public readonly SockChatServer Server;
        public readonly Timer BumpTimer;
        public readonly BanManager Bans = new BanManager();
        public readonly ChannelManager Channels = new ChannelManager();
        public readonly UserManager Users = new UserManager();
        public readonly ChatEventManager Events = new ChatEventManager();

        public ChatContext(SockChatServer server)
        {
            Server = server;
            BumpTimer = new Timer(e => BumpFlashiiOnline(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public void Update()
        {
            Bans.RemoveExpired();
            CheckPings();
        }

        public void BanUser(ChatUser user, DateTimeOffset? until = null, bool banIPs = false, UserDisconnectReason reason = UserDisconnectReason.Kicked)
        {
            if (until.HasValue && until.Value <= DateTimeOffset.UtcNow)
                until = null;

            if (until.HasValue)
            {
                user.Send(new ForceDisconnectPacket(ForceDisconnectReason.Banned, until.Value));
                Bans.Add(user, until.Value);

                if (banIPs)
                    lock (user.Connections)
                        foreach (IPAddress ip in user.RemoteAddresses)
                            Bans.Add(ip, until.Value);
            }
            else
                user.Send(new ForceDisconnectPacket(ForceDisconnectReason.Kicked));

            user.Close();
            UserLeave(user.Channel, user, reason);
        }

        public ChatChannel DefaultChannel
            => Channels.First();

        public string AddChannel(ChatChannel chan)
        {
            lock (Channels)
            {
                ChatChannel eChan = Channels.Get(chan.Name);
                if (eChan != null)
                    return ChatMessage.PackBotMessage(1, @"nischan", chan.Name);

                if (chan.Name.StartsWith(@"@") || chan.Name.StartsWith(@"*"))
                    return ChatMessage.PackBotMessage(1, @"inchan");

                Channels.Add(chan);

                lock (Users)
                    Users.Where(u => u.Hierarchy >= chan.Hierarchy).ForEach(u => u.Send(new ChannelCreatePacket(chan)));
            }

            return null;
        }

        public void DeleteChannel(ChatChannel chan)
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

        public void UpdateChannel(ChatChannel chan, string oldName = null)
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
            lock (Events)
                Events.Remove(msg);

            Broadcast(new ChatMessageDeletePacket(msg.MessageId));
        }

        public IEnumerable<IChatMessage> GetChannelBacklog(ChatChannel chan, int count = 15)
        {
            return Events.Where(x => x.Channel == chan || x.Channel == null).Reverse().Take(count).Reverse().ToArray();
        }

        public void HandleJoin(ChatUser user, ChatChannel chan, ChatUserConnection conn)
        {
            if (!chan.HasUser(user))
                chan.Send(new UserConnectPacket(DateTimeOffset.Now, user));

            conn.Send(new AuthSuccessPacket(user, chan));
            conn.Send(new ContextUsersPacket(chan.GetUsers(new[] { user })));

            IEnumerable<IChatMessage> msgs = GetChannelBacklog(chan);

            foreach (IChatMessage msg in msgs)
                conn.Send(new ContextMessagePacket(msg));

            lock (Channels)
                conn.Send(new ContextChannelsPacket(Channels.Where(x => user.Hierarchy >= x.Hierarchy)));

            if (!chan.HasUser(user))
                chan.UserJoin(user);

            if (!Users.Contains(user))
                Users.Add(user);
        }

        public void UserLeave(ChatChannel chan, ChatUser user, UserDisconnectReason reason = UserDisconnectReason.Leave)
        {
            if (chan == null)
            {
                Channels.Where(x => x.Users.Contains(user)).ToList().ForEach(x => UserLeave(x, user, reason));
                return;
            }

            if (chan.IsTemporary && chan.Owner == user)
                DeleteChannel(chan);

            chan.UserLeave(user);
            chan.Send(new UserDisconnectPacket(DateTimeOffset.Now, user, reason));
        }

        public void SwitchChannel(ChatUser user, string chanName, string password)
        {
            ChatChannel chan = Channels.Get(chanName);

            if (chan == null)
            {
                user.Send(true, @"nochan", chanName);
                user.ForceChannel();
                return;
            }

            SwitchChannel(user, chan, password);
        }

        public void SwitchChannel(ChatUser user, ChatChannel chan, string password)
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

        public void ForceChannelSwitch(ChatUser user, ChatChannel chan)
        {
            if (!Channels.Contains(chan))
                return;

            ChatChannel oldChan = user.Channel;

            oldChan.Send(new UserChannelLeavePacket(user));
            chan.Send(new UserChannelJoinPacket(user));

            user.Send(new ContextClearPacket(ContextClearMode.MessagesUsers));
            user.Send(new ContextUsersPacket(chan.GetUsers(new[] { user })));

            IEnumerable<IChatMessage> msgs = GetChannelBacklog(chan);

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
            List<ChatUser> users;

            lock(Users)
                users = new List<ChatUser>(Users);

            foreach (ChatUser user in users)
            {
                List<ChatUserConnection> conns = new List<ChatUserConnection>(user.Connections);

                foreach (ChatUserConnection conn in conns)
                {
                    if (conn.HasTimedOut)
                    {
                        user.Connections.Remove(conn);
                        conn.Dispose();
                        Logger.Write($@"Nuked a connection from {user.Username} {conn.HasTimedOut} {conn.Websocket.IsAvailable}");
                    }

                    if (user.Connections.Count < 1)
                        UserLeave(null, user, UserDisconnectReason.TimeOut);
                }
            }
        }

        public void BumpFlashiiOnline()
        {
            List<FlashiiBump> bups = new List<FlashiiBump>();

            lock (Users)
                Users.Where(u => u.IsAlive).ForEach(u => bups.Add(new FlashiiBump { UserId = u.UserId, UserIP = u.RemoteAddresses.First().ToString() }));

            if(bups.Any())
                FlashiiBump.Submit(bups);
        }

        public void Broadcast(IServerPacket packet)
        {
            lock (Users)
                Users.ForEach(u => u.Send(packet));
        }

        [Obsolete(@"Use Broadcast(IServerPacket, int)")]
        public void Broadcast(ChatUser user, string message, SockChatMessageFlags flags = SockChatMessageFlags.RegularUser)
        {
            lock (Users)
                Users.ForEach(u => u.Send(user, message, flags));
        }

        ~ChatContext()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing)
        {
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
