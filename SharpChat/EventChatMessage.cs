using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat
{
    public class EventChatMessage : IChatMessage
    {
        public int MessageId { get; set; }

        public string MessageIdStr { get; set; }

        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.UtcNow;

        public MessageFlags Flags { get; set; }

        public SockChatChannel Channel { get; set; }

        public SockChatUser User => SockChatServer.Bot;

        public string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendNum(IsError);
                sb.Append(Constants.MISC_SEPARATOR);
                sb.Append(EventName);

                foreach (object part in Parts)
                {
                    sb.Append(Constants.MISC_SEPARATOR);
                    sb.Append(part);
                }

                return sb.ToString();
            }
        }

        public bool IsError { get; set; }
        public string EventName { get; set; }
        public object[] Parts { get; set; }

        public EventChatMessage(SockChatChannel chan, int msgId, MessageFlags flags, bool error, string eventName, params object[] parts)
        {
            Channel = chan;
            MessageId = msgId;
            Flags = flags;
            IsError = error;
            EventName = eventName;
            Parts = parts;
        }
        public EventChatMessage(SockChatChannel chan, string msgId, MessageFlags flags, bool error, string eventName, params object[] parts)
        {
            Channel = chan;
            MessageIdStr = msgId;
            Flags = flags;
            IsError = error;
            EventName = eventName;
            Parts = parts;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(DateTime.ToUnixTimeSeconds());
            sb.Append('\t');
            sb.Append(User);
            sb.Append('\t');
            sb.Append(Text);
            sb.Append('\t');
            if (string.IsNullOrEmpty(MessageIdStr))
                sb.Append(MessageId);
            else
                sb.Append(MessageIdStr);
            sb.Append("\t0\t");
            sb.Append(Flags.Serialise());

            return sb.ToString();
        }

        // this is cursed

        public static EventChatMessage Info(int msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, MessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(int msgId, MessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, false, eventName, parts);
        public static EventChatMessage Info(SockChatChannel chan, int msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, MessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(SockChatChannel chan, int msgId, MessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, false, eventName, parts);
        public static EventChatMessage Info(string msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, MessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(string msgId, MessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, false, eventName, parts);
        public static EventChatMessage Info(SockChatChannel chan, string msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, MessageFlags.RegularUser, false, eventName, parts);
        public static EventChatMessage Info(SockChatChannel chan, string msgId, MessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, false, eventName, parts);

        public static EventChatMessage Error(int msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, MessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(int msgId, MessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, true, eventName, parts);
        public static EventChatMessage Error(SockChatChannel chan, int msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, MessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(SockChatChannel chan, int msgId, MessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, true, eventName, parts);
        public static EventChatMessage Error(string msgId, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, MessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(string msgId, MessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(null, msgId, flags, true, eventName, parts);
        public static EventChatMessage Error(SockChatChannel chan, string msgId, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, MessageFlags.RegularUser, true, eventName, parts);
        public static EventChatMessage Error(SockChatChannel chan, string msgId, MessageFlags flags, string eventName, params object[] parts)
            => new EventChatMessage(chan, msgId, flags, true, eventName, parts);
    }
}
