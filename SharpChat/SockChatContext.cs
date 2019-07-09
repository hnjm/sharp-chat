using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat
{
    public class SockChatContext : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public readonly SockChatServer Server;
        public readonly List<SockChatUser> Users = new List<SockChatUser>();
        public readonly List<SockChatChannel> Channels = new List<SockChatChannel>();
        public readonly List<SockChatMessage> Messages = new List<SockChatMessage>();

        public SockChatContext(SockChatServer server)
        {
            Server = server;
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
                    Users.Where(u => u.Hierarchy >= chan.Hierarchy).ForEach(u => u.Send(SockChatClientMessage.ChannelEvent, @"0", chan.ToString()));
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
                        Users.Where(u => u.Hierarchy >= chan.Hierarchy).ForEach(u => u.Send(SockChatClientMessage.ChannelEvent, @"2", chan.Name));
                        Channels.Remove(chan);
                    }
        }

        public void UpdateChannel(SockChatChannel chan, string oldName = null)
        {
            lock (Users)
                Users.Where(u => u.Hierarchy >= chan.Hierarchy).ForEach(u =>
                {
                    u.Send(SockChatClientMessage.ChannelEvent, @"1", oldName ?? chan.Name, chan.ToString());

                    /* Not entire sure how to recreate this behaviour at the moment
                if ($user->channel == $oldname && $oldname != "") {
                    $user->runSock(function($sock) use ($channel) {
                        $sock->send(pack_message(5, ["2", $channel->name]));
                    });
                    $user->channel = $channel->name;
                }
                     */
                });
        }

        public SockChatUser FindUserById(int userId)
        {
            return Users.FirstOrDefault(x => x.UserId == userId);
        }
        public SockChatUser FindUserByName(string name)
        {
            return Users.FirstOrDefault(x => x.Username.ToLowerInvariant() == name.ToLowerInvariant() || x.DisplayName.ToLowerInvariant() == name.ToLowerInvariant());
        }

        public SockChatUser FindUserBySock(IWebSocketConnection conn)
        {
            return Users.FirstOrDefault(x => x.Connections.Any(y => y.Websocket == conn));
        }

        public SockChatChannel FindChannelByName(string name)
        {
            return Channels.FirstOrDefault(x => x.Name.ToLowerInvariant().Trim() == name.ToLowerInvariant().Trim());
        }

        public SockChatChannel FindUserChannel(SockChatUser user)
        {
            return Channels.FirstOrDefault(c => c.Users.Contains(user));
        }

        public SockChatMessage[] GetChannelBacklog(SockChatChannel chan, int count = 15)
        {
            return Messages.Where(x => x.Channel == chan || x.Channel == null).Reverse().Take(count).Reverse().ToArray();
        }

        public void HandleJoin(SockChatUser user, SockChatChannel chan, IWebSocketConnection conn)
        {
            if (!chan.HasUser(user))
                chan.Send(SockChatClientMessage.UserConnect, Utils.UnixNow, user.ToString(), SockChatMessage.NextMessageId);

            conn.Send(SockChatClientMessage.UserConnect, @"y", user.ToString(), chan.Name);
            conn.Send(SockChatClientMessage.ContextPopulate, Constants.CTX_USER, chan.GetUsersString(new[] { user }));

            SockChatMessage[] msgs = GetChannelBacklog(chan);

            foreach (SockChatMessage msg in msgs)
                conn.Send(SockChatClientMessage.ContextPopulate, Constants.CTX_MSG, msg.GetLogString());

            SockChatChannel[] chans = Channels.Where(x => user.Hierarchy >= x.Hierarchy).ToArray();
            StringBuilder sb = new StringBuilder();

            sb.Append(chans.Length);

            foreach (SockChatChannel c in chans)
            {
                sb.Append('\t');
                sb.Append(c);
            }

            conn.Send(SockChatClientMessage.ContextPopulate, Constants.CTX_CHANNEL, sb.ToString());

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
            chan.Send(SockChatClientMessage.UserDisconnect, user.UserId.ToString(), user.Username, type, Utils.UnixNow, SockChatMessage.NextMessageId, chan.Name);
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

            string messageId = SockChatMessage.NextMessageId;
            SockChatChannel oldChan = user.Channel;

            oldChan.Send(SockChatClientMessage.UserSwitch, @"1", user.UserId.ToString(), messageId);
            chan.Send(SockChatClientMessage.UserSwitch, @"0", user.ToString(), messageId);

            user.Send(SockChatClientMessage.ContextClear, Constants.CLEAR_MSGNUSERS);
            user.Send(SockChatClientMessage.ContextPopulate, Constants.CTX_USER, chan.GetUsersString(new[] { user }));

            SockChatMessage[] msgs = GetChannelBacklog(chan);

            foreach (SockChatMessage msg in msgs)
                user.Send(SockChatClientMessage.ContextPopulate, Constants.CTX_MSG, msg.GetLogString());

            user.ForceChannel(chan);
            oldChan.UserLeave(user);
            chan.UserJoin(user);

            if(oldChan.IsTemporary && oldChan.Owner == user)
                DeleteChannel(oldChan);
        }

        public void CheckPings()
        {
            List<SockChatUser> users = new List<SockChatUser>(Users);

            foreach (SockChatUser user in users)
            {
                List<SockChatConn> conns = new List<SockChatConn>(user.Connections);

                foreach (SockChatConn conn in conns)
                {
                    if (conn.HasTimedOut)
                    {
                        user.Connections.Remove(conn);
                        conn.Close();
                        Logger.Write($@"Nuked a connection from {user.Username} {conn.HasTimedOut} {conn.Websocket.IsAvailable}");
                    }

                    if (user.Connections.Count < 1)
                        UserLeave(null, user, Constants.LEAVE_TIMEOUT);
                }
            }
        }

        public void Broadcast(SockChatUser user, string message, string flags = @"10010")
        {
            Channels.ForEach(c => c.Send(user, message, flags));
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

            Messages.Clear();
            Channels.Clear();
            Users.Clear();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
