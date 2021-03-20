using SharpChat.Channels;
using SharpChat.Messages;
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
        private ChannelManager Channels { get; }
        private IUser Sender { get; }
        private int Version { get; }

        public AuthPacketHandler(SessionManager sessions, ChannelManager channels, IUser sender, int version) {
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
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
                    ctx.Chat.DataProvider.BanClient.CheckBan(res.UserId, ctx.Connection.RemoteAddress, ban => {
                        if(ban.IsPermanent || ban.Expires > DateTimeOffset.Now) {
                            ctx.Connection.Send(new AuthFailPacket(AuthFailReason.Banned, ban));
                            ctx.Connection.Dispose();
                            return;
                        }

                        IUser user = ctx.Chat.Users.Connect(res);

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

                        IChannel chan = ctx.Chat.Channels.DefaultChannel;
                        bool shouldJoin = !ctx.Chat.ChannelUsers.HasUser(chan, user);

                        if(shouldJoin) {
                            chan.SendPacket(new UserConnectPacket(DateTimeOffset.Now, user));
                            //ctx.Chat.DispatchEvent(this, new UserConnectEvent(chan, user));
                        }

                        sess.SendPacket(new AuthSuccessPacket(user, chan, sess, Version, ctx.Chat.Messages.TextMaxLength));
                        ctx.Chat.ChannelUsers.GetUsers(chan, u => sess.SendPacket(new ContextUsersPacket(u.Except(new[] { user }).OrderByDescending(u => u.Rank))));

                        ctx.Chat.Messages.GetMessages(chan, m => {
                            foreach(IMessage msg in m)
                                sess.SendPacket(new ContextMessagePacket(msg));
                        });

                        ctx.Chat.Channels.GetChannels(user.Rank, c => sess.SendPacket(new ContextChannelsPacket(c)));

                        if(shouldJoin)
                            ctx.Chat.ChannelUsers.JoinChannel(chan, user);
                    }, exceptionHandler);
                },
                exceptionHandler
            );
        }
    }
}
