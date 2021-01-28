using SharpChat.Channels;
using SharpChat.Packets;
using SharpChat.Sessions;
using SharpChat.Users.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Users {
    public class ChatUser : User, IPacketTarget {
        public DateTimeOffset SilencedUntil { get; set; }

        private readonly List<Session> Sessions = new List<Session>();
        private readonly List<Channel> Channels = new List<Channel>();

        public readonly ChatRateLimiter RateLimiter = new ChatRateLimiter();

        public string TargetName => @"@log";

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
            Username = auth.Username;

            if(Status == UserStatus.Offline)
                Status = UserStatus.Online;

            Colour = auth.Colour;
            Rank = auth.Rank;
            Permissions = auth.Permissions;

            if(invalidateRestrictions || !IsSilenced)
                SilencedUntil = auth.SilencedUntil;
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
    }
}
