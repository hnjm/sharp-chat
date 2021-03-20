using SharpChat.Events;
using SharpChat.Users.Auth;
using System;
using System.Text;

namespace SharpChat.Users {
    public class User : IUser, IEventHandler {
        public long UserId { get; }
        public string UserName { get; private set; }
        public Colour Colour { get; private set; }
        public int Rank { get; private set; }
        public string NickName { get; private set; }
        public UserPermissions Permissions { get; private set; }
        public UserStatus Status { get; private set; } = UserStatus.Online;
        public string StatusMessage { get; private set; }

        public User(
            long userId,
            string userName,
            Colour colour,
            int rank,
            UserPermissions perms,
            UserStatus status,
            string statusMessage,
            string nickName
        ) {
            UserId = userId;
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Colour = colour;
            Rank = rank;
            Permissions = perms;
            Status = status;
            StatusMessage = statusMessage ?? string.Empty;
            NickName = nickName ?? string.Empty;
        }

        public User(IUserAuthResponse auth) {
            UserId = auth.UserId;
            ApplyAuth(auth);
        }

        public void ApplyAuth(IUserAuthResponse auth) {
            UserName = auth.UserName;

            if(Status == UserStatus.Offline)
                Status = UserStatus.Online;

            Colour = auth.Colour;
            Rank = auth.Rank;
            Permissions = auth.Permissions;
        }

        public bool Can(UserPermissions perm)
            => (Permissions & perm) == perm;

        public override string ToString()
            => $@"<ChatUser {UserId}#{UserName}>";

        public bool Equals(IUser other)
            => other != null && other.UserId == UserId;

        public void HandleEvent(object sender, IEvent evt) {
            switch(evt) {
                case UserUpdateEvent uue:
                    if(uue.HasUserName)
                        UserName = uue.UserName;
                    if(uue.Colour.HasValue)
                        Colour = uue.Colour.Value;
                    if(uue.Rank.HasValue)
                        Rank = uue.Rank.Value;
                    if(uue.HasNickName)
                        NickName = uue.NickName;
                    if(uue.Perms.HasValue)
                        Permissions = uue.Perms.Value;
                    if(uue.Status.HasValue)
                        Status = uue.Status.Value;
                    if(uue.HasStatusMessage)
                        StatusMessage = uue.StatusMessage;
                    break;

                case UserDisconnectEvent _:
                    Status = UserStatus.Offline;
                    break;
            }
        }
    }
}
