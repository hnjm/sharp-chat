using SharpChat.Users;
using System;
using System.Text.Json;

namespace SharpChat.Events {
    public interface IEvent {
        long EventId { get; }
        string Type { get; }
        DateTimeOffset DateTime { get; }
        IUser Sender { get; }
        string Target { get; }

        delegate IEvent DecodeFromJson(IEvent sourceEvent, JsonElement rootElement);
        string EncodeAsJson();
    }
}
