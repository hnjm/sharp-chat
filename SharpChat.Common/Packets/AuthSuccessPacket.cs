﻿using SharpChat.Channels;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class AuthSuccessPacket : ServerPacket {
        public IUser User { get; private set; }
        public IChannel Channel { get; private set; }
        public ISession Session { get; private set; }
        public int Version { get; private set; }
        public int CharacterLimit { get; private set; }

        public AuthSuccessPacket(IUser user, IChannel channel, ISession sess, int version, int charLimit) {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Session = sess ?? throw new ArgumentNullException(nameof(channel));
            Version = version;
            CharacterLimit = charLimit;
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.UserConnect);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append('y');
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.Pack());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Name);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Version);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Session.SessionId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(CharacterLimit);

            return sb.ToString();
        }
    }
}
