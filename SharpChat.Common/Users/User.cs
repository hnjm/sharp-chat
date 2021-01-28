using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SharpChat.Users {
    public class User : IEquatable<User> {
        public long UserId { get; set; }
        public string Username { get; set; }
        public ChatColour Colour { get; set; }
        public int Rank { get; set; }
        public string Nickname { get; set; }
        public UserPermissions Permissions { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Online;
        public string StatusMessage { get; set; }

        public bool Equals([AllowNull] User other)
            => UserId == other.UserId;
        public override bool Equals(object obj)
            => Equals(obj as User);
        public override int GetHashCode()
            => base.GetHashCode();

        public string DisplayName {
            get {
                StringBuilder sb = new StringBuilder();

                if(Status == UserStatus.Away)
                    sb.AppendFormat(@"&lt;{0}&gt;_", StatusMessage.Substring(0, Math.Min(StatusMessage.Length, 5)).ToUpperInvariant());

                if(string.IsNullOrWhiteSpace(Nickname))
                    sb.Append(Username);
                else {
                    sb.Append('~');
                    sb.Append(Nickname);
                }

                return sb.ToString();
            }
        }

        public bool Can(UserPermissions perm, bool strict = false) {
            UserPermissions perms = Permissions & perm;
            return strict ? perms == perm : perms > 0;
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append(UserId);
            sb.Append('\t');
            sb.Append(DisplayName);
            sb.Append('\t');
            sb.Append(Colour);
            sb.Append('\t');
            sb.Append(Rank);
            sb.Append(' ');
            sb.Append(Can(UserPermissions.KickUser) ? '1' : '0');
            sb.Append(@" 0 ");
            sb.Append(Can(UserPermissions.SetOwnNickname) ? '1' : '0');
            sb.Append(' ');
            sb.Append(Can(UserPermissions.CreateChannel | UserPermissions.SetChannelPermanent, true) ? 2 : (
                Can(UserPermissions.CreateChannel) ? 1 : 0
            ));

            return sb.ToString();
        }
    }
}
