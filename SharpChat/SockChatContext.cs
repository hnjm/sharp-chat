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

        public void AddChannel(SockChatChannel chan)
        {
            lock (Channels)
                Channels.Add(chan);
        }

        public void DeleteChannel(SockChatChannel chan)
        {
            if (chan.Name == @"Lounge")
                return;

            // move users to main channel

            lock (Channels)
                Channels.Remove(chan);

            // send deletion broadcast
        }

        public SockChatUser FindUserById(int userId)
        {
            return Users.FirstOrDefault(x => x.UserId == userId);
        }

        public SockChatUser FindUserBySock(IWebSocketConnection conn)
        {
            return Users.FirstOrDefault(x => x.Connections.Any(y => y.Websocket == conn));
        }

        public SockChatChannel FindChannelByName(string name)
        {
            return Channels.FirstOrDefault(x => x.Name.ToLowerInvariant().Trim() == name.ToLowerInvariant().Trim());
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

            //if (!chan.HasUser(user))
            //    LogToChannel(chan, user, SockChatMessage.PackBotMessage(0, @"join", user.Username), @"10010");

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
