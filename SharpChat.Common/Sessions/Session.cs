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

        public IConnection Connection { get; private set; }

        public string Id { get; private set; }
        public DateTimeOffset LastPing { get; set; }
        public ChatUser User { get; set; }

        public TimeSpan IdleTime => LastPing - DateTimeOffset.Now;

        public bool HasConnection
            => Connection != null;
        public bool HasUser
            => User != null;

        public IPAddress RemoteAddress
            => Connection?.RemoteAddress;

        private object Sync { get; } = new object();
        private Queue<IServerPacket> PacketQueue { get; } = new Queue<IServerPacket>();

        public Channel LastChannel { get; set; }

        public ClientCapabilities Capabilities { get; set; }

        public Session(IConnection conn, IHasSessions user) {
            Id = RNG.NextString(ID_LENGTH);
            BumpPing();
            Connection = conn;
            user.AddSession(this);
        }

        public bool HasCapability(ClientCapabilities capability)
            => (Capabilities & capability) == capability;

        public void SendPacket(IServerPacket packet) {
            lock(Sync) {
                if(!HasConnection) {
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

        public void ForceChannel(Channel channel = null) {
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
            User.RemoveSession(this);
            Connection.Dispose();
            LastPing = DateTimeOffset.MinValue;
        }
    }
}
