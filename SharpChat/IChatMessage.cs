using System;

namespace SharpChat
{
    [Flags]
    public enum MessageFlags
    {
        Bold = 1,
        Cursive = 1 << 1,
        Underline = 1 << 2,
        Colon = 1 << 3,
        Private = 1 << 4,

        RegularUser = Bold | Colon,
        RegularPM = RegularUser | Private,
        Action = Bold | Cursive,
    }

    public interface IChatMessage
    {
        int MessageId { get; }
        string Text { get; }
        DateTimeOffset DateTime { get; }
        MessageFlags Flags { get; }
        SockChatChannel Channel { get;  }
        SockChatUser User { get; }
        string ToString();
    }
}
