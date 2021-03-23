using SharpChat.Channels;
using SharpChat.DataProvider;
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

        public ClientPacketId PacketId => ClientPacketId.Authenticate;

        private SessionManager Sessions { get; }
        private UserManager Users { get; }
        private ChannelManager Channels { get; }
        private ChannelUserRelations ChannelUsers { get; }
        private MessageManager Messages { get; }
        private IDataProvider DataProvider { get; }
        private IUser Sender { get; }
        private int Version { get; }

        public AuthPacketHandler(
            SessionManager sessions,
            UserManager users,
            ChannelManager channels,
            ChannelUserRelations channelUsers,
            MessageManager messages,
            IDataProvider dataProvider,
            IUser sender,
            int version
        ) {
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            ChannelUsers = channelUsers ?? throw new ArgumentNullException(nameof(channelUsers));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
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

            DataProvider.UserAuthClient.AttemptAuth(
                new UserAuthRequest(userId, token, ctx.Connection.RemoteAddress),
                res => {
                    DataProvider.BanClient.CheckBan(res.UserId, ctx.Connection.RemoteAddress, ban => {
                        if(ban.IsPermanent || ban.Expires > DateTimeOffset.Now) {
                            ctx.Connection.Send(new AuthFailPacket(AuthFailReason.Banned, ban));
                            ctx.Connection.Dispose();
                            return;
                        }

                        IUser user = Users.Connect(res);

                        // Enforce a maximum amount of connections per user
                        if(Sessions.GetAvailableSessionCount(user) < 1) {
                            ctx.Connection.Send(new AuthFailPacket(AuthFailReason.MaxSessions));
                            ctx.Connection.Dispose();
                            return;
                        }

                        ILocalSession sess = Sessions.Create(ctx.Connection, user);

                        sess.SendPacket(new WelcomeMessagePacket(Sender, $@"Welcome to Flashii Chat, {user.UserName}!"));

                        if(File.Exists(WELCOME)) {
                            IEnumerable<string> lines = File.ReadAllLines(WELCOME).Where(x => !string.IsNullOrWhiteSpace(x));
                            string line = lines.ElementAtOrDefault(RNG.Next(lines.Count()));

                            if(!string.IsNullOrWhiteSpace(line))
                                sess.SendPacket(new WelcomeMessagePacket(Sender, line));
                        }

                        IChannel chan = Channels.DefaultChannel;
                        bool shouldJoin = !ChannelUsers.HasUser(chan, user);

                        if(shouldJoin) {
                            // ChannelUsers?
                            //chan.SendPacket(new UserConnectPacket(DateTimeOffset.Now, user));
                            //ctx.Chat.DispatchEvent(this, new UserConnectEvent(chan, user));
                        }

                        sess.SendPacket(new AuthSuccessPacket(user, chan, sess, Version, Messages.TextMaxLength));
                        ChannelUsers.GetUsers(chan, u => sess.SendPacket(new ContextUsersPacket(u.Except(new[] { user }).OrderByDescending(u => u.Rank))));

                        Messages.GetMessages(chan, m => {
                            foreach(IMessage msg in m)
                                sess.SendPacket(new ContextMessagePacket(msg));
                        });

                        Channels.GetChannels(user.Rank, c => sess.SendPacket(new ContextChannelsPacket(c)));

                        if(shouldJoin)
                            ChannelUsers.JoinChannel(chan, user);
                    }, exceptionHandler);
                },
                exceptionHandler
            );
        }
    }
}
