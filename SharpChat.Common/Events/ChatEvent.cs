using SharpChat.Channels;
using SharpChat.Users;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpChat.Events {
    public abstract class ChatEvent : IEvent {
        [JsonIgnore]
        public DateTimeOffset DateTime { get; set; }

        [JsonIgnore]
        public IUser Sender { get; set; }

        [JsonIgnore]
        public Channel Target { get; set; }

        [JsonIgnore]
        public string TargetName { get; set; }

        [JsonIgnore]
        public EventFlags Flags { get; set; }

        [JsonIgnore]
        public long SequenceId { get; set; }

        private const long SEQ_ID_EPOCH = 1588377600000;
        private static int SequenceIdCounter = 0;

        public static long GenerateSequenceId() {
            if(SequenceIdCounter > 200)
                SequenceIdCounter = 0;
            long id = 0;
            id |= (DateTimeOffset.Now.ToUnixTimeMilliseconds() - SEQ_ID_EPOCH) << 8;
            id |= (ushort)(++SequenceIdCounter);
            return id;
        }

        public ChatEvent(IEvent evt, JsonElement elem) {
            DateTime = evt.DateTime;
            Sender = evt.Sender;
            Target = evt.Target;
            TargetName = evt.TargetName;
            Flags = evt.Flags;
            SequenceId = evt.SequenceId;
        }
        public ChatEvent(DateTimeOffset dateTime, IUser user, Channel target, EventFlags flags) {
            SequenceId = GenerateSequenceId();
            DateTime = dateTime;
            Sender = user ?? throw new ArgumentNullException(nameof(user));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            TargetName = target.Name;
            Flags = flags;
        }
    }
}
