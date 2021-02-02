using SharpChat.Channels;
using SharpChat.Database;
using SharpChat.Users;
using System;

namespace SharpChat.Events.Storage {
    public class ADOEventReader : IEvent {
        public DateTimeOffset DateTime { get; }
        public IUser Sender { get; }
        public Channel Target { get; }
        public string TargetName { get; }
        public EventFlags Flags { get; }
        public long EventId { get; }

        public ADOEventReader(IDatabaseReader reader, Channel target) {
            EventId = reader.ReadI64(@"event_id");
            Target = target;
            TargetName = target?.Name ?? reader.ReadString(@"event_target");
            Flags = (EventFlags)reader.ReadU8(@"event_flags");
            DateTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadI32(@"event_created"));
            Sender = reader.IsNull(@"event_sender") ? null : new ADOUser(reader);
        }
    }
}
