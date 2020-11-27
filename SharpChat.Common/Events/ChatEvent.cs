using SharpChat.Users;
using System;
using System.Text.Json.Serialization;

namespace SharpChat.Events {
    public abstract class ChatEvent : IChatEvent {
        [JsonIgnore]
        public DateTimeOffset DateTime { get; set; }

        [JsonIgnore]
        public BasicUser Sender { get; set; }

        [JsonIgnore]
        public IPacketTarget Target { get; set; }

        [JsonIgnore]
        public string TargetName { get; set; }

        [JsonIgnore]
        public ChatEventFlags Flags { get; set; }

        [JsonIgnore]
        public long SequenceId { get; set; }

        private const long SEQ_ID_EPOCH = 1588377600000;
        private static int SequenceIdCounter = 0;

        public static long GenerateSequenceId() {
            if(SequenceIdCounter > 200)
                SequenceIdCounter = 0;
            long id = 0;
            id |= (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - SEQ_ID_EPOCH) << 8;
            id |= (ushort)(++SequenceIdCounter);
            return id;
        }

        public ChatEvent() { }
        public ChatEvent(DateTimeOffset dateTime, BasicUser user, IPacketTarget target, ChatEventFlags flags) {
            SequenceId = GenerateSequenceId();
            DateTime = dateTime;
            Sender = user;
            Target = target;
            TargetName = target?.TargetName;
            Flags = flags;
        }
    }
}
