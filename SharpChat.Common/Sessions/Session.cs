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
        public DateTimeOffset LastPing { get; private set; }
        public IUser User { get; private set; }

        public TimeSpan IdleTime => LastPing - DateTimeOffset.Now;

        public bool IsConnected
            => Connection != null;

        public IPAddress RemoteAddress
            => Connection?.RemoteAddress;

        private readonly object Sync = new object();
        private Queue<IServerPacket> PacketQueue { get; } = new Queue<IServerPacket>();

        private IChannel LastChannel { get; set; }

        public ClientCapability Capabilities { get; private set; }

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
            lock(Sync) {
                if(evt.Channel != null)
                    LastChannel = evt.Channel;

                switch(evt) {
                    case SessionCapabilitiesEvent sce:
                        Capabilities = sce.Capabilities;
                        SendPacket(new CapabilityConfirmationPacket(sce));
                        break;
                    case SessionPingEvent spe:
                        LastPing = spe.DateTime;
                        SendPacket(new PongPacket(spe));
                        break;
                    case SessionChannelSwitchEvent scwe:
                        if(scwe.Channel != null)
                            LastChannel = scwe.Channel;
                        SendPacket(new ChannelSwitchPacket(LastChannel));
                        break;

                    case ChannelJoinEvent cje:
                        SendPacket(new ChannelJoinPacket(cje));
                        break;
                    case ChannelLeaveEvent cle: // Should ownership just be passed on to another user instead of Destruction?
                        SendPacket(new ChannelLeavePacket(cle));
                        break;

                    case MessageCreateEvent mce:
                        SendPacket(new MessageCreatePacket(mce));
                        break;
                    case MessageUpdateEventWithData muewd:
                        SendPacket(new MessageDeletePacket(muewd));
                        SendPacket(new MessageCreatePacket(muewd));
                        break;
                    case MessageUpdateEvent mue:
                        //SendPacket(new MessageUpdatePacket(mue));
                        break;
                    case MessageDeleteEvent mde:
                        SendPacket(new MessageDeletePacket(mde));
                        break;

                    case BroadcastMessageEvent bme:
                        SendPacket(new BroadcastMessagePacket(bme));
                        break;
                }
            }
        }
    }
}
