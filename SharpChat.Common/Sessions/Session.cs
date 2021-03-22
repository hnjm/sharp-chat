using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharpChat.Sessions {
    public class Session : ILocalSession {
        public const int ID_LENGTH = 32;

        private IConnection Connection { get; set; }

        public string SessionId { get; }
        public string ServerId { get; }
        public DateTimeOffset LastPing { get; set; }
        public IUser User { get; set; }

        public TimeSpan IdleTime => LastPing - DateTimeOffset.Now;

        public bool IsConnected
            => Connection != null;

        public IPAddress RemoteAddress
            => Connection?.RemoteAddress;

        private object Sync { get; } = new object();
        private Queue<IServerPacket> PacketQueue { get; } = new Queue<IServerPacket>();

        public IChannel LastChannel { get; set; }

        public ClientCapabilities Capabilities { get; set; }

        public Session(string serverId, IConnection conn, IUser user) {
            ServerId = serverId ?? throw new ArgumentNullException(nameof(serverId));
            SessionId = RNG.NextString(ID_LENGTH);
            BumpPing();
            Connection = conn;
            User = user;
        }

        public bool HasConnection(IConnection conn)
            => Connection == conn;

        public void SendPacket(IServerPacket packet) {
            lock(Sync) {
                if(!IsConnected) {
                    PacketQueue.Enqueue(packet);
                    return;
                }

                if(!Connection.IsAvailable)
                    return;

                Connection.Send(packet.Pack());
            }
        }

        public void Suspend() {
            lock(Sync) {
                BumpPing();
                Connection = null;
            }
        }

        public void Resume(IConnection conn) {
            lock(Sync) {
                BumpPing();
                Connection = conn;

                while(PacketQueue.TryDequeue(out IServerPacket packet))
                    SendPacket(packet);
            }
        }

        public void BumpPing()
            => LastPing = DateTimeOffset.Now;

        public void ForceChannel(IChannel channel = null) {
            if(channel != null)
                LastChannel = channel;
            if(LastChannel == null)
                return;
            SendPacket(new ChannelForceJoinPacket(LastChannel));
        }

        public override string ToString() {
            return $@"S#{SessionId}";
        }

        private bool IsDisposed;
        ~Session()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if (IsDisposed)
                return;
            IsDisposed = true;
            Connection.Dispose();
            LastPing = DateTimeOffset.MinValue;
        }

        public bool Equals(ISession other)
            => other != null && ServerId.Equals(other.ServerId) && SessionId.Equals(other.SessionId);

        public void HandleEvent(object sender, IEvent evt) {
            throw new NotImplementedException();
        }
    }
}
