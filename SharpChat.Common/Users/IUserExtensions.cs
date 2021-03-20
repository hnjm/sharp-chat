using System;
using System.Text;

namespace SharpChat.Users {
    public static class IUserExtensions {
        public static bool Can(this IUser user, UserPermissions perm)
            => user is ChatBot || (user.Permissions & perm) == perm;

        public static string GetDisplayName(this IUser user) {
            if(user is ChatBot)
                return user.UserName;

            StringBuilder sb = new StringBuilder();

            if(user.Status == UserStatus.Away)
                sb.AppendFormat(@"&lt;{0}&gt;_", user.StatusMessage.Substring(0, Math.Min(user.StatusMessage.Length, 5)).ToUpperInvariant());

            if(string.IsNullOrWhiteSpace(user.NickName))
                sb.Append(user.UserName);
            else {
                sb.Append('~');
                sb.Append(user.NickName);
            }

            return sb.ToString();
        }

        public static string Pack(this IUser user) {
            if(user is ChatBot cb)
                return cb.PackBot();

            StringBuilder sb = new StringBuilder();
            user.Pack(sb);
            return sb.ToString();
        }

        public static void Pack(this IUser user, StringBuilder sb) {
            if(user is ChatBot cb) {
                sb.Append(cb.PackBot());
                return;
            }

            sb.Append(user.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(user.GetDisplayName());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(user.Colour);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(user.Rank);
            sb.Append(' ');
            sb.Append(user.Can(UserPermissions.KickUser) ? '1' : '0');
            sb.Append(@" 0 ");
            sb.Append(user.Can(UserPermissions.SetOwnNickname) ? '1' : '0');
            sb.Append(' ');
            sb.Append(user.Can(UserPermissions.CreateChannel | UserPermissions.SetChannelPermanent) ? 2 : (
                user.Can(UserPermissions.CreateChannel) ? 1 : 0
            ));
        }
    }
}
