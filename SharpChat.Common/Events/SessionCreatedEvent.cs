using SharpChat.Sessions;
using System;
using System.Net;

namespace SharpChat.Events {
    public class SessionCreatedEvent : Event {
        public const string TYPE = @"session:create";

        public override string Type => TYPE;
        public string ServerId { get; }
        public string SessionId { get; }
        public DateTimeOffset LastPing { get; }
        public bool IsConnected { get; }
        public IPAddress RemoteAddress { get; }
        public ClientCapabilities Capabilities { get; }

        public SessionCreatedEvent(ISession session) : base(null, session.User) {
            ServerId = session.ServerId;
            SessionId = session.SessionId;
            LastPing = session.LastPing;
            IsConnected = session.IsConnected;
            RemoteAddress = session.RemoteAddress;
            Capabilities = session.Capabilities;
        }
    }
}
