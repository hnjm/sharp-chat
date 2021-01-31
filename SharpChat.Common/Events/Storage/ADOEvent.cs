using SharpChat.Database;
using SharpChat.Users;
using System;

namespace SharpChat.Events.Storage {
    public class ADOEvent : IEvent {
        public Type Type { get; }
        public string Data { get; }

        public DateTimeOffset DateTime { get; }
        public IUser Sender { get; }
        public IPacketTarget Target { get; }
        public string TargetName { get; }
        public EventFlags Flags { get; }
        public long SequenceId { get; }

        public ADOEvent(IDatabaseReader reader, IPacketTarget target = null) {
            Type = Type.GetType(reader.ReadString(@"event_type"));
            SequenceId = reader.ReadI64(@"event_id");
            Target = target;
            TargetName = target?.TargetName ?? reader.ReadString(@"event_target");
            Flags = (EventFlags)reader.ReadU8(@"event_flags");
            DateTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadI32(@"event_created"));
            Sender = reader.IsNull(@"event_sender") ? null : new ADOUser(reader);
            Data = reader.ReadString(@"event_data");
        }
    }
}
