using SharpChat.Channels;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class AuthSuccessPacket : ServerPacket {
        public ChatUser User { get; private set; }
        public Channel Channel { get; private set; }
        public Session Session { get; private set; }
        public int CharacterLimit { get; private set; }

        public AuthSuccessPacket(ChatUser user, Channel channel, Session sess, int charLimit) {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Session = sess ?? throw new ArgumentNullException(nameof(channel));
            CharacterLimit = charLimit;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserConnect);
            sb.Append("\ty\t");
            sb.Append(User.Pack());
            sb.Append('\t');
            sb.Append(Channel.Name);
            sb.Append('\t');
            sb.Append(SockChatServer.EXT_VERSION);
            sb.Append('\t');
            sb.Append(Session.Id);
            sb.Append('\t');
            sb.Append(CharacterLimit);

            return new[] { sb.ToString() };
        }
    }
}
