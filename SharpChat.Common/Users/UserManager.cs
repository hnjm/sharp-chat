using SharpChat.Events;
using SharpChat.Sessions;
using SharpChat.Users.Auth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat.Users {
    public class UserManager : IEventHandler {
        private List<IUser> Users { get; } = new List<IUser>();
        private IEventDispatcher Dispatcher { get; }
        private IEventTarget Target { get; }
        private SessionManager Sessions { get; }
        private object Sync { get; } = new object();

        public UserManager(IEventDispatcher dispatcher, IEventTarget target, SessionManager sessions) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        }

        private void OnConnect(object sender, UserConnectEvent uce) {
            if(sender == this)
                return;

            lock(Sync) {
                if(Contains(uce.Sender))
                    throw new ArgumentException(@"User already registered?????", nameof(uce));

                Users.Add(new User(
                    uce.Sender.UserId,
                    uce.Sender.UserName,
                    uce.Sender.Colour,
                    uce.Sender.Rank,
                    uce.Sender.Permissions,
                    uce.Status,
                    uce.StatusMessage,
                    uce.Sender.NickName
                ));
            }
        }

        public void Disconnect(IUser user, UserDisconnectReason reason = UserDisconnectReason.Unknown) {
            if(user == null)
                return;
            lock(Sync)
                Dispatcher.DispatchEvent(this, new UserDisconnectEvent(Target, user, reason));
        }

        private void OnDisconnect(object sender, UserDisconnectEvent ude) {
            lock(Sync) {
                IUser user = Get(ude.Sender.UserId);
                if(user == null)
                    return;
                if(user is IEventHandler ueh)
                    ueh.HandleEvent(sender, ude);
                else
                    Users.Remove(user);
            }
        }

        private void OnUpdate(object sender, UserUpdateEvent uue) {
            lock(Sync) {
                IUser user = Get(uue.Sender.UserId);
                if(user is IEventHandler ueh)
                    ueh.HandleEvent(sender, uue);
            }
        }

        public bool Contains(IUser user) {
            if(user == null)
                return false;

            lock(Sync)
                return Users.Contains(user)
                    || Users.Any(x => x.Equals(user) || x.UserName.ToLowerInvariant() == user.UserName.ToLowerInvariant());
        }

        public IUser Get(long userId) {
            lock(Sync)
                return Users.FirstOrDefault(x => x.UserId == userId);
        }

        public IUser Get(string username, bool includeNickName = true, bool includeDisplayName = true) {
            if(string.IsNullOrWhiteSpace(username))
                return null;
            username = username.ToLowerInvariant();

            lock(Sync)
                return Users.FirstOrDefault(x => x.UserName.ToLowerInvariant() == username
                    || (includeNickName && x.NickName?.ToLowerInvariant() == username)
                    || (includeDisplayName && x.GetDisplayName().ToLowerInvariant() == username));
        }

        public IEnumerable<IUser> OfRank(int rank) {
            lock(Sync)
                return Users.Where(u => u.Rank >= rank).ToList();
        }

        public IEnumerable<IUser> WithActiveConnections() {
            lock(Sync)
                return Users.Where(u => Sessions.GetSessionCount(u) > 0).ToList();
        }

        public IEnumerable<IUser> All() {
            lock(Sync)
                return Users.ToList();
        }

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync)
                switch(evt) {
                    // Send to all users with minimum rank
                    case ChannelCreateEvent cce:
                        break;
                    case ChannelUpdateEvent cue:
                        break;
                    case ChannelDeleteEvent cde:
                        break;

                    case UserConnectEvent uce:
                        OnConnect(sender, uce);
                        break;
                    case UserUpdateEvent uue:
                        OnUpdate(sender, uue);
                        break;
                    case UserDisconnectEvent ude:
                        OnDisconnect(sender, ude);
                        break;
                }
        }

        public IUser Connect(IUserAuthResponse uar) {
            lock(Sync) {
                IUser user = Get(uar.UserId);
                if(user == null)
                    return Create(
                        uar.UserId,
                        uar.UserName,
                        uar.Colour,
                        uar.Rank,
                        uar.Permissions
                    );

                Update(user, uar.UserName, uar.Colour, uar.Rank, uar.Permissions);

                return user;
            }
        }

        private IUser Create(
            long userId,
            string userName,
            Colour colour,
            int rank,
            UserPermissions perms,
            UserStatus status = UserStatus.Online,
            string statusMessage = null,
            string nickName = null
        ) {
            lock(Sync) {
                IUser user = new User(userId, userName, colour, rank, perms, status, statusMessage, nickName);
                Users.Add(user);

                Dispatcher.DispatchEvent(this, new UserConnectEvent(Target, user));

                return user;
            }
        }

        public void Update(
            IUser user,
            string userName = null,
            Colour? colour = null,
            int? rank = null,
            UserPermissions? perms = null,
            UserStatus? status = null,
            string statusMessage = null,
            string nickName = null
        ) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(!Users.Contains(user))
                throw new ArgumentException(@"Provided user is not registered with this manager.", nameof(user));

            lock(Sync) {
                if(userName != null && user.UserName == userName)
                    userName = null;

                if(colour.HasValue && user.Colour.Equals(colour))
                    colour = null;

                if(rank.HasValue && user.Rank == rank.Value)
                    rank = null;

                if(nickName != null) {
                    string prevNickName = user.NickName ?? string.Empty;

                    if(nickName == prevNickName) {
                        nickName = null;
                    } else {
                        string nextUserName = userName ?? user.UserName;
                        if(nickName == nextUserName) {
                            nickName = null;
                        } else {
                            // cleanup
                        }
                    }
                }

                if(perms.HasValue && user.Permissions == perms.Value)
                    perms = null;

                if(status.HasValue && user.Status == status.Value)
                    status = null;

                if(statusMessage != null && user.StatusMessage == statusMessage) {
                    statusMessage = null;
                } else {
                    // cleanup
                }

                Dispatcher.DispatchEvent(this, new UserUpdateEvent(Target, user, userName, colour, rank, nickName, perms, status, statusMessage));
            }
        }
    }
}
