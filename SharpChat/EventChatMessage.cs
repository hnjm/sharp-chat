using System;
using System.Text;

namespace SharpChat
{
    public class EventChatMessage : IChatMessage
    {
        public string MessageIdStr { get; set; }
        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.UtcNow;
        public ChatMessageFlags Flags { get; set; } = ChatMessageFlags.None;
        public IPacketTarget Target { get; set; }
        public int SequenceId { get; set; }
        public ChatUser Sender => SockChatServer.Bot;

        public string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(IsError ? '1' : '0');
                sb.Append('\f');
                sb.Append(EventName);

                foreach (object part in Parts)
                {
                    sb.Append('\f');
                    sb.Append(part);
                }

                return sb.ToString();
            }
        }

        public bool IsError { get; set; }
        public string EventName { get; set; }
        public object[] Parts { get; set; }

        public EventChatMessage(ChatChannel chan, ChatMessageFlags flags, bool error, string eventName, params object[] parts)
        {
            Target = chan;
            Flags = flags;
            IsError = error;
            EventName = eventName;
            Parts = parts;
        }
        public EventChatMessage(ChatChannel chan, string msgId, ChatMessageFlags flags, bool error, string eventName, params object[] parts)
        {
            Target = chan;
            MessageIdStr = msgId;
            Flags = flags;
            IsError = error;
            EventName = eventName;
            Parts = parts;
        }

        // this is cursed

        public static EventChatMessage Info(string eventName, params object[] parts)
            => new EventChatMessage(null, ChatMessageFlags.None, false, eventName, parts);
        public static EventChatMessage Info(ChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, flags, false, eventName, parts);
        public static EventChatMessage Info(ChatChannel chan, string eventName, params object[] parts)
            => new EventChatMessage(chan, ChatMessageFlags.None, false, eventName, parts);
        public static EventChatMessage Info(ChatChannel chan, ChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, flags, false, eventName, parts);
        public static EventChatMessage Info(string msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, ChatMessageFlags.None, false, eventName, parts);
        public static EventChatMessage Info(string msgId, ChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, false, eventName, parts);
        public static EventChatMessage Info(ChatChannel chan, string msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, ChatMessageFlags.None, false, eventName, parts);
        public static EventChatMessage Info(ChatChannel chan, string msgId, ChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, false, eventName, parts);

        public static EventChatMessage Error(string eventName, params object[] parts)
            => new EventChatMessage(null, ChatMessageFlags.None, true, eventName, parts);
        public static EventChatMessage Error(ChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, flags, true, eventName, parts);
        public static EventChatMessage Error(ChatChannel chan, string eventName, params object[] parts)
            => new EventChatMessage(chan, ChatMessageFlags.None, true, eventName, parts);
        public static EventChatMessage Error(ChatChannel chan, ChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, flags, true, eventName, parts);
        public static EventChatMessage Error(string msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, ChatMessageFlags.None, true, eventName, parts);
        public static EventChatMessage Error(string msgId, ChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, true, eventName, parts);
        public static EventChatMessage Error(ChatChannel chan, string msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, ChatMessageFlags.None, true, eventName, parts);
        public static EventChatMessage Error(ChatChannel chan, string msgId, ChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, true, eventName, parts);
    }
}
