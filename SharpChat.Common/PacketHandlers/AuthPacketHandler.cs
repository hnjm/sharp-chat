using SharpChat.Channels;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Sessions;
using SharpChat.Users;
using SharpChat.Users.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpChat.PacketHandlers {
    public class AuthPacketHandler : IPacketHandler {
        private const string WELCOME = @"welcome.txt";

        public ClientPacket PacketId => ClientPacket.Authenticate;

        private SessionManager Sessions { get; }
        private IUser Sender { get; }
        private int Version { get; }

        public AuthPacketHandler(SessionManager sessions, IUser sender, int version) {
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Version = version;
        }

        public void HandlePacket(IPacketHandlerContext ctx) {
            if(ctx.HasSession)
                return;

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long userId) || userId < 1)
                return;

            string token = ctx.Args.ElementAtOrDefault(2);
            if(string.IsNullOrEmpty(token))
                return;

            Action<Exception> exceptionHandler = new Action<Exception>(ex => {
                Logger.Debug($@"<{ctx.Connection.RemoteAddress}> Auth fail: {ex.Message}");
                ctx.Connection.Send(new AuthFailPacket(AuthFailReason.AuthInvalid));
                ctx.Connection.Dispose();
            });

            ctx.Chat.DataProvider.UserAuthClient.AttemptAuth(
                new UserAuthRequest(userId, token, ctx.Connection.RemoteAddress),
                res => {
                    ChatUser user = ctx.Chat.Users.Get(res.UserId);

                    if(user == null)
                        user = new ChatUser(res);
                    else {
                        user.ApplyAuth(res);

                        IServerPacket userUpdate = new UserUpdatePacket(user);
                        foreach(Channel uc in user.GetChannels())
                            uc.SendPacket(userUpdate);
                    }

                    ctx.Chat.DataProvider.BanClient.CheckBan(user.UserId, ctx.Connection.RemoteAddress, ban => {
                        if(ban.IsPermanent || ban.Expires > DateTimeOffset.Now) {
                            ctx.Connection.Send(new AuthFailPacket(AuthFailReason.Banned, ban));
                            ctx.Connection.Dispose();
                            return;
                        }

                        // Enforce a maximum amount of connections per user
                        if(Sessions.GetAvailableSessionCount(user) < 1) {
                            ctx.Connection.Send(new AuthFailPacket(AuthFailReason.MaxSessions));
                            ctx.Connection.Dispose();
                            return;
                        }

                        Session sess = new Session(ctx.Connection, user);
                        Sessions.Add(sess);

                        sess.SendPacket(new WelcomeMessagePacket(Sender, $@"Welcome to Flashii Chat, {user.UserName}!"));

                        if(File.Exists(WELCOME)) {
                            IEnumerable<string> lines = File.ReadAllLines(WELCOME).Where(x => !string.IsNullOrWhiteSpace(x));
                            string line = lines.ElementAtOrDefault(RNG.Next(lines.Count()));

                            if(!string.IsNullOrWhiteSpace(line))
                                sess.SendPacket(new WelcomeMessagePacket(Sender, line));
                        }

                        Channel chan = ctx.Chat.Channels.DefaultChannel;

                        if(!chan.HasUser(user)) {
                            chan.SendPacket(new UserConnectPacket(DateTimeOffset.Now, user));
                            ctx.Chat.Events.DispatchEvent(new UserConnectEvent(DateTimeOffset.Now, user, chan));
                        }

                        sess.SendPacket(new AuthSuccessPacket(user, chan, sess, Version, ctx.Chat.MessageTextMaxLength));
                        sess.SendPacket(new ContextUsersPacket(chan.GetUsers(new[] { user })));

                        IEnumerable<IEvent> msgs = ctx.Chat.Events.GetEventsForTarget(chan);

                        foreach(IEvent msg in msgs)
                            sess.SendPacket(new ContextMessagePacket(msg));

                        sess.SendPacket(new ContextChannelsPacket(ctx.Chat.Channels.OfHierarchy(user.Rank)));

                        if(!chan.HasUser(user))
                            chan.UserJoin(user);

                        if(!ctx.Chat.Users.Contains(user))
                            ctx.Chat.Users.Add(user);
                    }, exceptionHandler);
                },
                exceptionHandler
            );
        }
    }
}
