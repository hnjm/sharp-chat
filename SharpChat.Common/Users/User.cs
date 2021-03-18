using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Users.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Users {
    public class User : IUser, IEventHandler {
        public long UserId { get; }
        public string UserName { get; private set; }
        public Colour Colour { get; private set; }
        public int Rank { get; private set; }
        public string NickName { get; private set; }
        public UserPermissions Permissions { get; private set; }
        public UserStatus Status { get; private set; } = UserStatus.Online;
        public string StatusMessage { get; private set; }

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

        public User(
            long userId,
            string userName,
            Colour colour,
            int rank,
            UserPermissions perms,
            UserStatus status,
            string statusMessage,
            string nickName
        ) {
            UserId = userId;
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Colour = colour;
            Rank = rank;
            Permissions = perms;
            Status = status;
            StatusMessage = statusMessage ?? string.Empty;
            NickName = nickName ?? string.Empty;
        }

        public User(IUserAuthResponse auth) {
            UserId = auth.UserId;
            ApplyAuth(auth, true);
        }

        public void ApplyAuth(IUserAuthResponse auth, bool invalidateRestrictions = false) {
            UserName = auth.UserName;

            if(Status == UserStatus.Offline)
                Status = UserStatus.Online;

            Colour = auth.Colour;
            Rank = auth.Rank;
            Permissions = auth.Permissions;
        }

        public bool Can(UserPermissions perm)
            => (Permissions & perm) == perm;

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

        public void SendPacket(IServerPacket packet) {
            /*lock(SyncSessions)
                foreach(Session conn in Sessions)
                    conn.SendPacket(packet);*/
        }

        public void Close() {
            /*lock(SyncSessions) {
                while(Sessions.Any())
                    Sessions.First().Dispose();
            }*/
        }

        public void ForceChannel(IChannel chan = null) {
            /*lock(SyncSessions) {
                foreach(Session session in Sessions)
                    session.ForceChannel(chan);
            }*/
        }

        public bool InChannel(IChannel chan) {
            /*lock(SyncChannels)
                return Channels.Contains(chan);*/
            return false;
        }

        public void JoinChannel(IChannel chan) {
            /*lock(SyncChannels)
                if(!InChannel(chan))
                    Channels.Add(chan);*/
        }

        public void LeaveChannel(IChannel chan) {
            /*lock(SyncChannels)
                Channels.Remove(chan);*/
        }

        public IEnumerable<IChannel> GetChannels() {
            /*lock(SyncChannels)
                return Channels.ToList();*/
            return null;
        }

        public override string ToString() {
            return $@"<ChatUser {UserId}#{UserName}>";
        }

        public bool Equals(IUser other)
            => other != null && other.UserId == UserId;

        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case UserUpdateEvent uue:
                    if(uue.HasUserName)
                        UserName = uue.UserName;
                    if(uue.Colour.HasValue)
                        Colour = uue.Colour.Value;
                    if(uue.Rank.HasValue)
                        Rank = uue.Rank.Value;
                    if(uue.HasNickName)
                        NickName = uue.NickName;
                    if(uue.Perms.HasValue)
                        Permissions = uue.Perms.Value;
                    if(uue.Status.HasValue)
                        Status = uue.Status.Value;
                    if(uue.HasStatusMessage)
                        StatusMessage = uue.StatusMessage;
                    break;

                case UserDisconnectEvent _:
                    Status = UserStatus.Offline;
                    break;
            }
        }
    }
}
