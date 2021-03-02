using SharpChat.Database;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Events.Storage {
    public class ADOUser : IUser {
        public long UserId { get; }
        public string UserName { get; }
        public Colour Colour { get; }
        public int Rank { get; }
        public string NickName { get; }
        public UserPermissions Permissions { get; }
        public UserStatus Status => UserStatus.Unknown;
        public string StatusMessage => string.Empty;

        public string DisplayName {
            get {
                StringBuilder sb = new StringBuilder();

                if(string.IsNullOrWhiteSpace(NickName))
                    sb.Append(UserName);
                else {
                    sb.Append('~');
                    sb.Append(NickName);
                }

                return sb.ToString();
            }
        }

        public ADOUser(IDatabaseReader reader) {
            if(reader == null)
                throw new ArgumentNullException(nameof(reader));
            UserId = reader.ReadI64(@"event_sender");
            UserName = reader.ReadString(@"event_sender_name");
            Colour = new Colour(reader.ReadI32(@"event_sender_colour"));
            Rank = reader.ReadI32(@"event_sender_rank");
            NickName = reader.IsNull(@"event_sender_nick") ? null : reader.ReadString(@"event_sender_nick");
            Permissions = (UserPermissions)reader.ReadI32(@"event_sender_perms");
        }

        public bool Can(UserPermissions perm)
            => (Permissions & perm) == perm;

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append(UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(DisplayName);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Colour);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Rank);
            sb.Append(' ');
            sb.Append(Can(UserPermissions.KickUser) ? '1' : '0');
            sb.Append(@" 0 ");
            sb.Append(Can(UserPermissions.SetOwnNickname) ? '1' : '0');
            sb.Append(' ');
            sb.Append(Can(UserPermissions.CreateChannel | UserPermissions.SetChannelPermanent) ? 2 : (
                Can(UserPermissions.CreateChannel) ? 1 : 0
            ));

            return sb.ToString();
        }

        public override string ToString() {
            return $@"<ADOUser {UserId}#{UserName}>";
        }
    }
}
