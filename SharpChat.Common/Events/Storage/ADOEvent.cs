using SharpChat.Database;
using SharpChat.Users;
using System;

namespace SharpChat.Events.Storage {
    public class ADOEvent : IEvent {
        public long EventId { get; }
        public string Type { get; }
        public DateTimeOffset DateTime { get; }
        public IUser Sender { get; }
        public string Target { get; }
        public string RawData { get; }

        public ADOEvent(IDatabaseReader reader) {
            EventId = reader.ReadI64(@"event_id");
            Type = reader.ReadString(@"event_type");
            DateTime = DateTimeOffset.FromUnixTimeSeconds(reader.ReadI32(@"event_created"));
            Sender = reader.IsNull(@"event_sender") ? null : new ADOUser(reader);
            Target = reader.ReadString(@"event_target");
            RawData = reader.ReadString(@"event_data");
        }

        public string EncodeAsJson()
            => RawData;
    }
}
