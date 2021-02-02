using SharpChat.Users;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharpChat.Sessions {
    public class Session : IDisposable, IPacketTarget {
        public const int ID_LENGTH = 32;

        public IConnection Connection { get; private set; }

        public string Id { get; private set; }
        public DateTimeOffset LastPing { get; set; }
        public ChatUser User { get; set; }

        public TimeSpan IdleTime => DateTimeOffset.Now - LastPing;

        public bool HasConnection
            => Connection != null;

        public IPAddress RemoteAddress
            => Connection?.RemoteAddress;

        private object Sync { get; } = new object();
        private Queue<IServerPacket> PacketQueue { get; } = new Queue<IServerPacket>();

        public Session(IConnection conn, IHasSessions user) {
            Id = RNG.NextString(ID_LENGTH);
            BumpPing();
            Connection = conn;
            user.AddSession(this);
        }

        public void Send(IServerPacket packet) {
            lock(Sync) {
                if(!HasConnection) {
                    PacketQueue.Enqueue(packet);
                    return;
                }

                if(!Connection.IsAvailable)
                    return;

                IEnumerable<string> data = packet.Pack();

                if(data != null)
                    foreach(string line in data)
                        if(!string.IsNullOrWhiteSpace(line))
                            Connection.Send(line);
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
                    Send(packet);
            }
        }

        public void BumpPing()
            => LastPing = DateTimeOffset.Now;

        public override string ToString() {
            return Id;
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
