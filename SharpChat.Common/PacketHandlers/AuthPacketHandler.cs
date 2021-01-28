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

        public SockChatClientPacket PacketId => SockChatClientPacket.Authenticate;

        private SessionManager Sessions { get; }

        public AuthPacketHandler(SessionManager sessions) {
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        }

        public void HandlePacket(IPacketHandlerContext ctx) {
            if(ctx.HasUser)
                return;

            DateTimeOffset banDuration = ctx.Chat.Bans.Check(ctx.Session.RemoteAddress);

            if(banDuration > DateTimeOffset.Now) {
                ctx.Session.Send(new AuthFailPacket(AuthFailReason.Banned, banDuration));
                ctx.Session.Dispose();
                return;
            }

            if(!long.TryParse(ctx.Args.ElementAtOrDefault(1), out long userId) || userId < 1)
                return;

            string token = ctx.Args.ElementAtOrDefault(2);
            if(string.IsNullOrEmpty(token))
                return;

            ctx.Chat.DataProvider.UserAuthClient.AttemptAuth(
                new UserAuthRequest(userId, token, ctx.Session.RemoteAddress),
                onSuccess: res => {
                    ChatUser user = ctx.Chat.Users.Get(res.UserId);

                    if(user == null)
                        user = new ChatUser(res);
                    else {
                        user.ApplyAuth(res);
                        user.Channel?.Send(new UserUpdatePacket(user));
                    }

                    banDuration = ctx.Chat.Bans.Check(user);

                    if(banDuration > DateTimeOffset.Now) {
                        ctx.Session.Send(new AuthFailPacket(AuthFailReason.Banned, banDuration));
                        ctx.Session.Dispose();
                        return;
                    }

                    // Enforce a maximum amount of connections per user
                    if(Sessions.GetAvailableSessionCount(user) < 1) {
                        ctx.Session.Send(new AuthFailPacket(AuthFailReason.MaxSessions));
                        ctx.Session.Dispose();
                        return;
                    }

                    // Bumping the ping to prevent upgrading
                    ctx.Session.BumpPing();

                    user.AddSession(ctx.Session);

                    ctx.Session.Send(new LegacyCommandResponse(LCR.WELCOME, false, $@"Welcome to Flashii Chat, {user.Username}!"));

                    if(File.Exists(WELCOME)) {
                        IEnumerable<string> lines = File.ReadAllLines(WELCOME).Where(x => !string.IsNullOrWhiteSpace(x));
                        string line = lines.ElementAtOrDefault(RNG.Next(lines.Count()));

                        if(!string.IsNullOrWhiteSpace(line))
                            ctx.Session.Send(new LegacyCommandResponse(LCR.WELCOME, false, line));
                    }

                    ctx.Chat.HandleJoin(user, ctx.Chat.Channels.DefaultChannel, ctx.Session);
                },
                onFailure: ex => {
                    Logger.Debug($@"<{ctx.Session.Id}> Auth fail: {ex.Message}");
                    ctx.Session.Send(new AuthFailPacket(AuthFailReason.AuthInvalid));
                    ctx.Session.Dispose();
                }
            );
        }
    }
}
