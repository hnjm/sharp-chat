using SharpChat.Channels;
using SharpChat.Packets;
using SharpChat.Sessions;
using SharpChat.Users.Auth;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpChat.Users {
    public class ChatUser : IUser, IHasSessions, IPacketTarget {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public Colour Colour { get; set; }
        public int Rank { get; set; }
        public string NickName { get; set; }
        public UserPermissions Permissions { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Online;
        public string StatusMessage { get; set; }

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

        [Obsolete(@"Don't rely on this anymore, keep multi-channel in mind.")]
        public Channel Channel {
            get {
                lock(Channels)
                    return Channels.FirstOrDefault();
            }
        }

        // This needs to be a session thing
        public Channel CurrentChannel { get; private set; }

        public bool IsSilenced
            => DateTimeOffset.Now - SilencedUntil <= TimeSpan.Zero;

        public IEnumerable<IPAddress> RemoteAddresses {
            get {
                lock(Sessions)
                    return Sessions.Select(c => c.RemoteAddress).Distinct().ToArray();
            }
        }

        public ChatUser() { }
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

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append(UserId);
            sb.Append('\t');
            sb.Append(DisplayName);
            sb.Append('\t');
            sb.Append(Colour);
            sb.Append('\t');
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
            lock(Sessions)
                foreach(Session conn in Sessions)
                    conn.Send(packet);
        }

        public void Close() {
            lock(Sessions) {
                foreach(Session conn in Sessions)
                    conn.Dispose();
                Sessions.Clear();
            }
        }

        public void ForceChannel(Channel chan = null)
            => Send(new UserChannelForceJoinPacket(chan ?? CurrentChannel));

        public void FocusChannel(Channel chan) {
            lock(Channels) {
                if(InChannel(chan))
                    CurrentChannel = chan;
            }
        }

        public bool InChannel(Channel chan) {
            lock(Channels)
                return Channels.Contains(chan);
        }

        public void JoinChannel(Channel chan) {
            lock(Channels) {
                if(!InChannel(chan)) {
                    Channels.Add(chan);
                    CurrentChannel = chan;
                }
            }
        }

        public void LeaveChannel(Channel chan) {
            lock(Channels) {
                Channels.Remove(chan);
                CurrentChannel = Channels.FirstOrDefault();
            }
        }

        public IEnumerable<Channel> GetChannels() {
            lock(Channels)
                return Channels.ToList();
        }

        public void AddSession(Session sess) {
            if(sess == null)
                return;
            sess.User = this;
            lock(Sessions)
                Sessions.Add(sess);
        }

        public void RemoveSession(Session sess) {
            if(sess == null)
                return;
            sess.User = null;
            lock(Sessions)
                Sessions.Remove(sess);
        }

        public bool HasSession(Session sess) {
            if(sess == null)
                throw new ArgumentNullException(nameof(sess));
            lock(Sessions)
                return Sessions.Contains(sess);
        }

        public bool HasConnection(IConnection conn) {
            if(conn == null)
                throw new ArgumentNullException(nameof(conn));
            lock(Sessions)
                return Sessions.Any(s => s.Connection == conn);
        }
    }
}
