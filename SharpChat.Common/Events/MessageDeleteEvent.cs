using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpChat.Events {
    public class MessageDeleteEvent : Event, IDeleteEvent {
        public const string TYPE = @"message:delete";

        public override string Type => TYPE;
        public long TargetId { get; }

        public MessageDeleteEvent(IEventTarget target, IUser actor, long eventId)
            : base(target, actor) {
            TargetId = eventId;
        }
        public MessageDeleteEvent(IEventTarget target, IUser actor, MessageCreateEvent message)
            : this(target, actor, (message ?? throw new ArgumentNullException(nameof(message))).EventId) { }
        private MessageDeleteEvent(IEvent evt, long targetId) : base(evt) {
            TargetId = targetId;
        }

        public override string EncodeAsJson() {
            return JsonSerializer.Serialize(new Dictionary<string, object> {
                { @"id", TargetId },
            });
        }

        public static IEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            long eventId = 0;
            if(elem.TryGetProperty(@"id", out JsonElement idElem) && !idElem.TryGetInt64(out eventId))
                eventId = 0;

            return new MessageDeleteEvent(evt, eventId);
        }
    }
}
