﻿using System;
using System.Text;

namespace SharpChat
{
    public class EventChatMessage : IChatMessage
    {
        public int MessageId { get; set; }
        public string MessageIdStr { get; set; }
        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.UtcNow;
        public SockChatMessageFlags Flags { get; set; }
        public ChatChannel Channel { get; set; }
        public ChatUser User => SockChatServer.Bot;

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

        public EventChatMessage(ChatChannel chan, int msgId, SockChatMessageFlags flags, bool error, string eventName, params object[] parts)
        {
            Channel = chan;
            MessageId = msgId;
            Flags = flags;
            IsError = error;
            EventName = eventName;
            Parts = parts;
        }
        public EventChatMessage(ChatChannel chan, string msgId, SockChatMessageFlags flags, bool error, string eventName, params object[] parts)
        {
            Channel = chan;
            MessageIdStr = msgId;
            Flags = flags;
            IsError = error;
            EventName = eventName;
            Parts = parts;
        }

        // this is cursed

        public static EventChatMessage Info(int msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, SockChatMessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(int msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, false, eventName, parts);
        public static EventChatMessage Info(ChatChannel chan, int msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, SockChatMessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(ChatChannel chan, int msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, false, eventName, parts);
        public static EventChatMessage Info(string msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, SockChatMessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(string msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, false, eventName, parts);
        public static EventChatMessage Info(ChatChannel chan, string msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, SockChatMessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(ChatChannel chan, string msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, false, eventName, parts);

        public static EventChatMessage Error(int msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, SockChatMessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(int msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, true, eventName, parts);
        public static EventChatMessage Error(ChatChannel chan, int msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, SockChatMessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(ChatChannel chan, int msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, true, eventName, parts);
        public static EventChatMessage Error(string msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, SockChatMessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(string msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, true, eventName, parts);
        public static EventChatMessage Error(ChatChannel chan, string msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, SockChatMessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(ChatChannel chan, string msgId, SockChatMessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, true, eventName, parts);
    }
}
