using SharpChat.Flashii;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpChat {
    public class ChatUser : IPacketTarget {
        public int UserId { get; set; }
        public string Username { get; set; }
        public ChatColour Colour { get; set; }
        public int Hierarchy { get; set; }
        public string Nickname { get; set; }
        public ChatUserPermissions Permissions { get; set; }

        public ChatUserStatus Status { get; set; } = ChatUserStatus.Online;
        public string StatusMessage { get; set; }

        public DateTimeOffset SilencedUntil { get; set; }

        public readonly List<ChatUserConnection> Connections = new List<ChatUserConnection>();

        public string TargetName => @"@log";

        public ChatChannel Channel {
            get {
                lock (Channels)
                    return Channels.FirstOrDefault();
            }
        }

        public readonly ChatRateLimiter RateLimiter = new ChatRateLimiter();

        public readonly List<ChatChannel> Channels = new List<ChatChannel>();

        public bool IsSilenced
            => SilencedUntil != null && DateTimeOffset.UtcNow - SilencedUntil <= TimeSpan.Zero;

        public bool IsAlive
            => Connections.Where(c => !c.HasTimedOut).Any();

        public IEnumerable<IPAddress> RemoteAddresses
            => Connections.Select(c => c.RemoteAddress);

        public ChatUser() {
        }

        public ChatUser(FlashiiAuth auth) {
            UserId = auth.UserId;
            ApplyAuth(auth, true);
        }

        public string GetDisplayName(int version, bool forceOriginal = false) {
            StringBuilder sb = new StringBuilder();

            if (version < 2 && Status == ChatUserStatus.Away)
                sb.AppendFormat(@"&lt;{0}&gt;_", StatusMessage.Substring(0, Math.Min(StatusMessage.Length, 5)).ToUpperInvariant());

            if (forceOriginal || string.IsNullOrWhiteSpace(Nickname))
                sb.Append(Username);
            else {
                if (version < 2)
                    sb.Append('~');

                sb.Append(Nickname);
            }

            return sb.ToString();
        }

        public bool Can(ChatUserPermissions perm, bool strict = false) {
            ChatUserPermissions perms = Permissions & perm;
            return strict ? perms == perm : perms > 0;
        }

        public void ApplyAuth(FlashiiAuth auth, bool invalidateRestrictions = false) {
            Username = auth.Username;

            if (Status == ChatUserStatus.Offline)
                Status = ChatUserStatus.Online;
            
            Colour = new ChatColour(auth.ColourRaw);
            Hierarchy = auth.Hierarchy;
            Permissions = auth.Permissions;

            if (invalidateRestrictions || !IsSilenced)
                SilencedUntil = auth.SilencedUntil;
        }

        public void Send(IServerPacket packet) {
            lock (Connections)
                Connections.ForEach(c => c.Send(packet));
        }

        public void Close() {
            lock (Connections) {
                Connections.ForEach(c => c.Dispose());
                Connections.Clear();
            }
        }

        public void ForceChannel(ChatChannel chan = null)
            => Send(new UserChannelForceJoinPacket(chan ?? Channel));

        public void AddConnection(ChatUserConnection conn) {
            lock (Connections)
                Connections.Add(conn);

            conn.User = this;
        }

        public void RemoveConnection(ChatUserConnection conn) {
            conn.User = null;

            lock (Connections)
                Connections.Remove(conn);
        }

        public string Pack(int targetVersion) {
            StringBuilder sb = new StringBuilder();

            sb.Append(UserId);
            sb.Append('\t');
            sb.Append(GetDisplayName(targetVersion));
            sb.Append('\t');
            if (targetVersion >= 2)
                sb.Append(Colour.Raw);
            else
                sb.Append(Colour);
            sb.Append('\t');
            sb.Append(Hierarchy);
            sb.Append(' ');
            sb.Append(Can(ChatUserPermissions.KickUser) ? '1' : '0');
            sb.Append(@" 0 ");
            sb.Append(Can(ChatUserPermissions.SetOwnNickname) ? '1' : '0');
            sb.Append(' ');
            sb.Append(Can(ChatUserPermissions.CreateChannel | ChatUserPermissions.SetChannelPermanent, true) ? 2 : (
                Can(ChatUserPermissions.CreateChannel) ? 1 : 0
            ));

            return sb.ToString();
        }
    }
}
