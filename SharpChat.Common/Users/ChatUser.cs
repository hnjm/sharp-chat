using SharpChat.Channels;
using SharpChat.Sessions;
using SharpChat.Users.Auth;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpChat.Users {
    public class ChatUser : IUser, IHasSessions, IServerPacketTarget {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public Colour Colour { get; set; }
        public int Rank { get; set; }
        public string NickName { get; set; }
        public UserPermissions Permissions { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Online;
        public string StatusMessage { get; set; }

        private object SyncChannels { get; } = new object();
        private object SyncSessions { get; } = new object();

        public string DisplayName {
            get {
                StringBuilder sb = new StringBuilder();

                if(Status == UserStatus.Away)
                    sb.AppendFormat(@"&lt;{0}&gt;_", StatusMessage.Substring(0, Math.Min(StatusMessage.Length, 5)).ToUpperInvariant());

                if(string.IsNullOrWhiteSpace(NickName))
                    sb.Append(UserName);
                else {
                    sb.Append('~');
                    sb.Append(NickName);
                }

                return sb.ToString();
            }
        }

        public DateTimeOffset SilencedUntil { get; set; }

        private List<Session> Sessions { get; } = new List<Session>();
        private List<Channel> Channels { get; } = new List<Channel>();

        public bool IsSilenced
            => DateTimeOffset.Now - SilencedUntil <= TimeSpan.Zero;

        public IEnumerable<IPAddress> RemoteAddresses {
            get {
                lock(SyncSessions)
                    return Sessions.Select(c => c.RemoteAddress).Distinct().ToArray();
            }
        }

        public ChatUser(IUserAuthResponse auth) {
            UserId = auth.UserId;
            ApplyAuth(auth, true);
        }

        public void ApplyAuth(IUserAuthResponse auth, bool invalidateRestrictions = false) {
            UserName = auth.Username;

            if(Status == UserStatus.Offline)
                Status = UserStatus.Online;

            Colour = auth.Colour;
            Rank = auth.Rank;
            Permissions = auth.Permissions;

            if(invalidateRestrictions || !IsSilenced)
                SilencedUntil = auth.SilencedUntil;
        }

        public bool Can(UserPermissions perm)
            => (Permissions & perm) == perm;

        public bool HasCapability(ClientCapabilities capability) {
            lock(SyncSessions)
                return Sessions.Any(s => s.HasCapability(capability));
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append(UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(DisplayName);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Colour);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Rank);
            sb.Append(' ');
            sb.Append(Can(UserPermissions.KickUser) ? '1' : '0');
            sb.Append(@" 0 ");
            sb.Append(Can(UserPermissions.SetOwnNickname) ? '1' : '0');
            sb.Append(' ');
            sb.Append(Can(UserPermissions.CreateChannel | UserPermissions.SetChannelPermanent) ? 2 : (
                Can(UserPermissions.CreateChannel) ? 1 : 0
            ));

            return sb.ToString();
        }

        public void Send(IServerPacket packet) {
            lock(SyncSessions)
                foreach(Session conn in Sessions)
                    conn.Send(packet);
        }

        public void Close() {
            lock(SyncSessions) {
                while(Sessions.Any())
                    Sessions.First().Dispose();
            }
        }

        public void ForceChannel(Channel chan = null) {
            lock(SyncSessions) {
                foreach(Session session in Sessions)
                    session.ForceChannel(chan);
            }
        }

        public bool InChannel(Channel chan) {
            lock(SyncChannels)
                return Channels.Contains(chan);
        }

        public void JoinChannel(Channel chan) {
            lock(SyncChannels) {
                if(!InChannel(chan))
                    Channels.Add(chan);
            }
        }

        public void LeaveChannel(Channel chan) {
            lock(SyncChannels) {
                Channels.Remove(chan);
            }
        }

        public IEnumerable<Channel> GetChannels() {
            lock(SyncChannels)
                return Channels.ToList();
        }

        public void AddSession(Session sess) {
            if(sess == null)
                return;
            sess.User = this;
            lock(SyncSessions)
                Sessions.Add(sess);
        }

        public void RemoveSession(Session sess) {
            if(sess == null)
                return;
            sess.User = null;
            lock(SyncSessions)
                Sessions.Remove(sess);
        }

        public bool HasSession(Session sess) {
            if(sess == null)
                throw new ArgumentNullException(nameof(sess));
            lock(SyncSessions)
                return Sessions.Contains(sess);
        }

        public bool HasConnection(IConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            lock(SyncSessions)
                return Sessions.Any(s => s.Connection == conn);
        }
    }
}
