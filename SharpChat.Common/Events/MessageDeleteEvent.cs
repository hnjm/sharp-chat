using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class MessageDeleteEvent : Event {
        public const string TYPE = @"message:delete";

        public override string Type => TYPE;
        public long MessageId { get; }

        public MessageDeleteEvent(IEventTarget target, IUser actor, long eventId)
            : base(target, actor) {
            MessageId = eventId;
        }
        public MessageDeleteEvent(IEventTarget target, IUser actor, MessageCreateEvent message)
            : this(target, actor, (message ?? throw new ArgumentNullException(nameof(message))).EventId) { }

        public override string EncodeAsJson() {
            return JsonSerializer.Serialize(new Dictionary<string, object> {
                { @"id", MessageId },
            });
        }
    }
}
