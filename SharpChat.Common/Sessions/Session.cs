using SharpChat.Channels;
using SharpChat.Packets;
using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharpChat.Sessions {
    public class Session : IDisposable, IServerPacketTarget {
        public const int ID_LENGTH = 32;

        private IConnection Connection { get; set; }

        public string Id { get; private set; }
        public DateTimeOffset LastPing { get; set; }
        public IUser User { get; set; }

        public TimeSpan IdleTime => LastPing - DateTimeOffset.Now;

        public DateTimeOffset LastActivity { get; private set; } = DateTimeOffset.MinValue;

        public bool IsConnected
            => Connection != null;
        public bool HasUser
            => User != null;

        public IPAddress RemoteAddress
            => Connection?.RemoteAddress;

        private object Sync { get; } = new object();
        private Queue<IServerPacket> PacketQueue { get; } = new Queue<IServerPacket>();

        public IChannel LastChannel { get; set; }

        public ClientCapabilities Capabilities { get; set; }

        public Session(IConnection conn, IUser user) {
            Id = RNG.NextString(ID_LENGTH);
            BumpPing();
            Connection = conn;
            User = user;
        }

        public bool HasConnection(IConnection conn)
            => Connection == conn;

        public bool HasCapability(ClientCapabilities capability)
            => (Capabilities & capability) == capability;

        public void SendPacket(IServerPacket packet) {
            lock(Sync) {
                if(!IsConnected) {
                    PacketQueue.Enqueue(packet);
                    return;
                }

                if(!Connection.IsAvailable)
                    return;

                LastActivity = DateTimeOffset.Now;
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
            SendPacket(new UserChannelForceJoinPacket(LastChannel));
        }

        public override string ToString() {
            return $@"S#{Id}";
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
    }
}
