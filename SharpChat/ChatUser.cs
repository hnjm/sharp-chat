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

        private readonly string[] TextSuffixes = new[] {
            @"desu", @"desu wa", @"desu wa ne", @"desu yo",
            @"nya", @"nyo", @"nyu", @"nyoron", @"da ze", @"nanodesu",
            @"de gozaru", @"desu no"
        };

        private string TextSuffixValue = null;
        public string TextSuffix {
            get {
                if(TextSuffixValue == null) {
                    StringBuilder sb = new StringBuilder();

                    Random rng = new Random(0x51DEB00B | UserId);

                    sb.Append(TextSuffixes[rng.Next() % TextSuffixes.Length]);

                    if (rng.Next(0, 100) >= 50)
                        sb.Append('~');

                    TextSuffixValue = sb.ToString();
                }

                return TextSuffixValue;
            }
        }

        public string NameSuffix {
            get {
                if (Hierarchy >= 10)
                    return @"-sama";
                if (Hierarchy >= 5)
                    return @"-sensei";
                if (Colour.Raw == 0xF02D7D)
                    return @"-san";
                if (Colour.Raw == 0x0099FF)
                    return @"-wan";

                switch(UserId % 3) {
                    default:
                        return @"-chan";
                    case 1:
                        return @"-tan";
                    case 2:
                        return @"-kun";
                }
            }
        }

        public ChatUserStatus Status { get; set; } = ChatUserStatus.Online;
        public string StatusMessage { get; set; }
        public DateTimeOffset SilencedUntil { get; set; }

        private readonly List<ChatUserConnection> Connections = new List<ChatUserConnection>();
        private readonly List<ChatChannel> Channels = new List<ChatChannel>();

        public readonly ChatRateLimiter RateLimiter = new ChatRateLimiter();

        public string TargetName => @"@log";

        public ChatChannel Channel {
            get {
                lock(Channels)
                    return Channels.FirstOrDefault();
            }
        }

        public bool IsSilenced
            => SilencedUntil != null && DateTimeOffset.UtcNow - SilencedUntil <= TimeSpan.Zero;

        public bool HasConnections {
            get {
                lock(Connections)
                    return Connections.Where(c => !c.HasTimedOut && !c.IsDisposed).Any();
            }
        }

        public int ConnectionCount {
            get {
                lock (Connections)
                    return Connections.Where(c => !c.HasTimedOut && !c.IsDisposed).Count();
            }
        }

        public IEnumerable<IPAddress> RemoteAddresses {
            get {
                lock(Connections)
                    return Connections.Select(c => c.RemoteAddress);
            }
        }

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

            sb.Append(NameSuffix);

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
            lock(Connections)
                foreach (ChatUserConnection conn in Connections)
                    conn.Send(packet);
        }

        public void Close() {
            lock (Connections) {
                foreach (ChatUserConnection conn in Connections)
                    conn.Dispose();
                Connections.Clear();
            }
        }

        public void ForceChannel(ChatChannel chan = null)
            => Send(new UserChannelForceJoinPacket(chan ?? Channel));

        public bool InChannel(ChatChannel chan) {
            lock (Channels)
                return Channels.Contains(chan);
        }

        public void JoinChannel(ChatChannel chan) {
            lock (Channels) {
                if (!InChannel(chan))
                    Channels.Add(chan);
            }
        }

        public void LeaveChannel(ChatChannel chan) {
            lock (Channels)
                Channels.Remove(chan);
        }

        public IEnumerable<ChatChannel> GetChannels() {
            lock (Channels)
                return Channels.ToList();
        }

        public void AddConnection(ChatUserConnection conn) {
            if (conn == null)
                return;
            conn.User = this;

            lock (Connections)
                Connections.Add(conn);
        }

        public void RemoveConnection(ChatUserConnection conn) {
            if (conn == null)
                return;
            if(!conn.IsDisposed) // this could be possible
                conn.User = null;

            lock(Connections)
                Connections.Remove(conn);
        }

        public IEnumerable<ChatUserConnection> GetDeadConnections() {
            lock (Connections)
                return Connections.Where(x => x.HasTimedOut || x.IsDisposed).ToList();
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
