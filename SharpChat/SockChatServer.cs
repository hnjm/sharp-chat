using Fleck;
using SharpChat.Events;
using SharpChat.Flashii;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SharpChat {
    public class SockChatServer : IDisposable {
        public const int VERSION =
#if DEBUG
            2;
#else
            1;
#endif

        public const int MAX_CONNECTIONS =
#if DEBUG
            9001;
#else
            5;
#endif

        public const int FLOOD_KICK_LENGTH =
#if DEBUG
            5;
#else
            30;
#endif

        public const int MSG_LENGTH_MAX = 5000;

        public bool IsDisposed { get; private set; }

        public static readonly ChatUser Bot = new ChatUser {
            UserId = -1,
            Username = @"ChatBot",
            Hierarchy = 0,
            Colour = new ChatColour(),
        };

        public readonly WebSocketServer Server;
        public readonly ChatContext Context;

        public readonly List<ChatUserConnection> Connections = new List<ChatUserConnection>();
        private readonly object ConnectionsLock = new object();

        public ChatUserConnection GetConnection(IWebSocketConnection conn) {
            lock(ConnectionsLock)
                return Connections.FirstOrDefault(x => x.Websocket == conn);
        }

        public SockChatServer(ushort port) {
            Logger.Write("Starting Sock Chat server...");

            Context = new ChatContext(this);

            Context.Channels.Add(new ChatChannel(@"Lounge"));
#if DEBUG
            Context.Channels.Add(new ChatChannel(@"Programming"));
            Context.Channels.Add(new ChatChannel(@"Games"));
            Context.Channels.Add(new ChatChannel(@"Splatoon"));
            Context.Channels.Add(new ChatChannel(@"Password") { Password = @"meow", });
#endif
            Context.Channels.Add(new ChatChannel(@"Staff") { Hierarchy = 5 });

            Server = new WebSocketServer($@"ws://0.0.0.0:{port}");
            
            Server.Start(sock => {
                sock.OnOpen = () => OnOpen(sock);
                sock.OnClose = () => OnClose(sock);
                sock.OnError = err => OnError(sock, err);
                sock.OnMessage = msg => OnMessage(sock, msg);
            });
        }

        private void OnOpen(IWebSocketConnection ws) {
            lock (ConnectionsLock) {
                if (!Connections.Any(x => x.Websocket == ws))
                    Connections.Add(new ChatUserConnection(ws));
            }

            Context.Update();
        }

        private void OnClose(IWebSocketConnection ws) {
            ChatUserConnection conn = GetConnection(ws);

            // Remove connection from user
            if (conn?.User != null) {
                // RemoveConnection sets conn.User to null so we must grab a local copy.
                ChatUser user = conn.User;

                user.RemoveConnection(conn);

                if (!user.HasConnections)
                    Context.UserLeave(null, user);
            }

            // Update context
            Context.Update();

            // Remove connection from server
            lock(ConnectionsLock)
                Connections.Remove(conn);

            conn?.Dispose();
        }

        private void OnError(IWebSocketConnection conn, Exception ex) {
            Logger.Write($@"[{conn.ConnectionInfo.ClientIpAddress}] {ex}");
            Context.Update();
        }

        private void OnMessage(IWebSocketConnection ws, string msg) {
            Context.Update();

            ChatUserConnection conn = GetConnection(ws);

            if (conn == null) {
                ws.Close();
                return;
            }

            if (conn.User != null) {
                conn.User.RateLimiter.AddTimePoint();

                if (conn.User.RateLimiter.State == ChatRateLimitState.Kick) {
                    Context.BanUser(conn.User, DateTimeOffset.UtcNow.AddSeconds(FLOOD_KICK_LENGTH), false, UserDisconnectReason.Flood);
                    return;
                } else if (conn.User.RateLimiter.State == ChatRateLimitState.Warning)
                    conn.User.Send(new FloodWarningPacket()); // make it so this thing only sends once
            }

            string[] args = msg.Split('\t');

            if (args.Length < 1 || !Enum.TryParse(args[0], out SockChatClientPacket opCode))
                return;

            switch (opCode) {
                case SockChatClientPacket.Ping:
                    if (!int.TryParse(args[1], out int pTime))
                        break;

                    conn.BumpPing();
                    conn.Send(new PongPacket(conn.LastPing));
                    break;

                case SockChatClientPacket.Authenticate:
                    if (conn.User != null)
                        break;

                    DateTimeOffset aBanned = Context.Bans.Check(conn.RemoteAddress);

                    if (aBanned > DateTimeOffset.UtcNow) {
                        conn.Send(new AuthFailPacket(AuthFailReason.Banned, aBanned));
                        conn.Dispose();
                        break;
                    }

                    ChatUser aUser;
                    FlashiiAuth auth;

                    aUser = conn.User;

                    if (aUser != null || args.Length < 3 || !int.TryParse(args[1], out int aUserId))
                        break;

                    auth = FlashiiAuth.Attempt(new FlashiiAuthRequest {
                        UserId = aUserId,
                        Token = args[2],
                        IPAddress = conn.RemoteAddress.ToString(),
                    });

                    if (!auth.Success) {
                        //conn.Websocket.Send($"100\t{auth.Reason}");
                        conn.Send(new AuthFailPacket(AuthFailReason.AuthInvalid));
                        conn.Dispose();
                        break;
                    }

                    aUser = Context.Users.Get(auth.UserId);

                    if (aUser == null)
                        aUser = new ChatUser(auth);
                    else {
                        aUser.ApplyAuth(auth);
                        aUser.Channel?.Send(new UserUpdatePacket(aUser));
                    }

                    aBanned = Context.Bans.Check(aUser);

                    if (aBanned > DateTimeOffset.Now) {
                        conn.Send(new AuthFailPacket(AuthFailReason.Banned, aBanned));
                        conn.Dispose();
                        break;
                    }

                    // Enforce a maximum amount of connections per user
                    if (aUser.ConnectionCount >= MAX_CONNECTIONS) {
                        conn.Send(new AuthFailPacket(AuthFailReason.MaxSessions));
                        conn.Dispose();
                        break;
                    }

                    // Bumping the ping to prevent upgrading
                    conn.BumpPing();

                    aUser.AddConnection(conn);

                    if (conn.Version < 2) {
                        conn.Send(new LegacyCommandResponse(LCR.WELCOME, false, $@"Welcome to Flashii Chat, {aUser.Username}!"));

                        if (File.Exists(@"welcome.txt")) {
                            IEnumerable<string> lines = File.ReadAllLines(@"welcome.txt").Where(x => !string.IsNullOrWhiteSpace(x));
                            string line = lines.ElementAtOrDefault(RNG.Next(lines.Count()));

                            if (!string.IsNullOrWhiteSpace(line))
                                conn.Send(new LegacyCommandResponse(LCR.WELCOME, false, line));
                        }
                    }

                    Context.HandleJoin(aUser, Context.Channels.DefaultChannel, conn);
                    break;

                case SockChatClientPacket.MessageSend:
                    if (args.Length < 3)
                        break;

                    ChatUser mUser = conn.User;
                    ChatChannel mChannel;

                    if (mUser == null || !mUser.Can(ChatUserPermissions.SendMessage) || string.IsNullOrWhiteSpace(args[2]))
                        break;

                    if (conn.Version < 2) {
#if !DEBUG
                    // Extra validation step, not necessary at all but enforces proper formatting in SCv1.
                    if (!int.TryParse(args[1], out int mUserId) || mUser.UserId != mUserId)
                        break;
#endif
                        mChannel = Context.Channels.GetUser(mUser).FirstOrDefault();
                    } else
                        mChannel = Context.Channels.Get(args[1]);

                    if (mChannel == null
                        || !mUser.InChannel(mChannel)
                        || (mUser.IsSilenced && !mUser.Can(ChatUserPermissions.SilenceUser)))
                        break;

                    if (mUser.Status != ChatUserStatus.Online) {
                        mUser.Status = ChatUserStatus.Online;
                        mChannel.Send(new UserUpdatePacket(mUser));
                    }

                    string messageText = string.Join('\t', args.Skip(2));

                    if (messageText.Length > MSG_LENGTH_MAX)
                        messageText = messageText.Substring(0, MSG_LENGTH_MAX);

                    messageText = messageText.Trim();

#if DEBUG
                    Logger.Write($@"<{mUser.Username}> {messageText}");
#endif

                    ChatMessage message = null;

                    // These commands are only available in V1, all server side commands are to be replaced with packets and client side commands.
                    if (conn.Version < 2 && messageText[0] == '/') {
                        message = HandleV1Command(messageText, mUser, mChannel);

                        if (message == null)
                            break;
                    }

                    if (message == null)
                        message = new ChatMessage {
                            Target = mChannel,
                            DateTime = DateTimeOffset.UtcNow,
                            Sender = mUser,
                            Text = messageText,
                        };

                    Context.Events.Add(message);
                    mChannel.Send(new ChatMessageAddPacket(message));
                    break;

                case SockChatClientPacket.Upgrade:
#pragma warning disable CS0162
                    if (conn.User != null || conn.LastPing != DateTimeOffset.MinValue || VERSION < 2)
                        break;
#pragma warning restore CS0162

                    if (!int.TryParse(args[1], out int uVersion) || uVersion < 2 || uVersion > VERSION) {
                        conn.Send(new UpgradeAckPacket(false, VERSION));
                        break;
                    }

                    conn.Version = uVersion;
                    conn.Send(new UpgradeAckPacket(true, conn.Version));
                    conn.BumpPing();
                    break;

                case SockChatClientPacket.Typing:
                    if (conn.Version < 2 || conn.User == null)
                        break;

                    Logger.Write($@"Typing packet received from {conn.User.UserId} {conn.User.Username}");
                    break;
            }
        }

        public ChatMessage HandleV1Command(string message, ChatUser user, ChatChannel channel) {
            string[] parts = message.Substring(1).Split(' ');
            string command = parts[0].Replace(@".", string.Empty).ToLowerInvariant();

            for (int i = 1; i < parts.Length; i++)
                parts[i] = parts[i].Replace(@"<", @"&lt;")
                                   .Replace(@">", @"&gt;")
                                   .Replace("\n", @" <br/> ")
                                   .Replace("\t", @"    ");

            switch (command) {
                case @"afk": // go afk
                    string afkStr = parts.Length < 2 || string.IsNullOrEmpty(parts[1])
                        ? @"AFK"
                        : string.Join(' ', parts.Skip(1));

                    if (!string.IsNullOrEmpty(afkStr)) {
                        user.Status = ChatUserStatus.Away;
                        user.StatusMessage = afkStr.Substring(0, Math.Min(afkStr.Length, 100)).Trim();
                        channel.Send(new UserUpdatePacket(user));
                    }
                    break;
                case @"nick": // sets a temporary nickname
                    bool setOthersNick = user.Can(ChatUserPermissions.SetOthersNickname);

                    if (!setOthersNick && !user.Can(ChatUserPermissions.SetOwnNickname)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    ChatUser targetUser = null;
                    int offset = 1;

                    if (setOthersNick && int.TryParse(parts[1], out int targetUserId) && targetUserId > 0) {
                        targetUser = Context.Users.Get(targetUserId);
                        offset = 2;
                    }

                    if (targetUser == null)
                        targetUser = user;

                    if(parts.Length < offset) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                        break;
                    }

                    string nickStr = string.Join('_', parts.Skip(offset))
                        .Replace(' ', '_')
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty)
                        .Replace("\f", string.Empty)
                        .Replace("\t", string.Empty)
                        .Trim();

                    if (nickStr == targetUser.Username)
                        nickStr = null;
                    else if (nickStr.Length > 15)
                        nickStr = nickStr.Substring(0, 15);
                    else if (string.IsNullOrEmpty(nickStr))
                        nickStr = null;

                    if (nickStr != null && Context.Users.Get(nickStr) != null) {
                        user.Send(new LegacyCommandResponse(LCR.NAME_IN_USE, true, nickStr));
                        break;
                    }

                    string previousName = targetUser == user ? (targetUser.Nickname ?? targetUser.Username) : null;
                    targetUser.Nickname = nickStr;
                    channel.Send(new UserUpdatePacket(targetUser, previousName));
                    break;
                case @"whisper": // sends a pm to another user
                case @"msg":
                    if (parts.Length < 3) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                        break;
                    }

                    ChatUser whisperUser = Context.Users.Get(parts[1]);

                    if (whisperUser == null) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_FOUND, true, parts[1]));
                        break;
                    }

                    if (whisperUser == user)
                        break;

                    string whisperStr = string.Join(' ', parts.Skip(2));

                    whisperUser.Send(new ChatMessageAddPacket(new ChatMessage {
                        DateTime = DateTimeOffset.Now,
                        Target = whisperUser,
                        Sender = user,
                        Text = whisperStr,
                        Flags = ChatMessageFlags.Private,
                    }));
                    user.Send(new ChatMessageAddPacket(new ChatMessage {
                        DateTime = DateTimeOffset.Now,
                        Target = whisperUser,
                        Sender = user,
                        Text = $@"{whisperUser.GetDisplayName(1)} {whisperStr}",
                        Flags = ChatMessageFlags.Private,
                    }));
                    break;
                case @"action": // describe an action
                case @"me":
                    if (parts.Length < 2)
                        break;

                    string actionMsg = string.Join(' ', parts.Skip(1));

                    return new ChatMessage {
                        Target = channel,
                        DateTime = DateTimeOffset.UtcNow,
                        Sender = user,
                        Text = actionMsg,
                        Flags = ChatMessageFlags.Action,
                    };
                case @"who": // gets all online users/online users in a channel if arg
                    StringBuilder whoChanSB = new StringBuilder();
                    string whoChanStr = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? parts[1] : string.Empty;

                    if (!string.IsNullOrEmpty(whoChanStr)) {
                        ChatChannel whoChan = Context.Channels.Get(whoChanStr);

                        if (whoChan == null) {
                            user.Send(new LegacyCommandResponse(LCR.CHANNEL_NOT_FOUND, true, whoChanStr));
                            break;
                        }

                        if (whoChan.Hierarchy > user.Hierarchy || (whoChan.HasPassword && !user.Can(ChatUserPermissions.JoinAnyChannel))) {
                            user.Send(new LegacyCommandResponse(LCR.USERS_LISTING_ERROR, true, whoChanStr));
                            break;
                        }

                        foreach (ChatUser whoUser in whoChan.GetUsers()) {
                            whoChanSB.Append(@"<a href=""javascript:void(0);"" onclick=""UI.InsertChatText(this.innerHTML);""");

                            if (whoUser == user)
                                whoChanSB.Append(@" style=""font-weight: bold;""");

                            whoChanSB.Append(@">");
                            whoChanSB.Append(whoUser.GetDisplayName(1));
                            whoChanSB.Append(@"</a>, ");
                        }

                        if (whoChanSB.Length > 2)
                            whoChanSB.Length -= 2;

                        user.Send(new LegacyCommandResponse(LCR.USERS_LISTING_CHANNEL, false, whoChanSB));
                    } else {
                        foreach (ChatUser whoUser in Context.Users.All()) {
                            whoChanSB.Append(@"<a href=""javascript:void(0);"" onclick=""UI.InsertChatText(this.innerHTML);""");

                            if (whoUser == user)
                                whoChanSB.Append(@" style=""font-weight: bold;""");

                            whoChanSB.Append(@">");
                            whoChanSB.Append(whoUser.GetDisplayName(1));
                            whoChanSB.Append(@"</a>, ");
                        }

                        if (whoChanSB.Length > 2)
                            whoChanSB.Length -= 2;

                        user.Send(new LegacyCommandResponse(LCR.USERS_LISTING_SERVER, false, whoChanSB));
                    }
                    break;

                // double alias for delchan and delmsg
                // if the argument is a number we're deleting a message
                // if the argument is a string we're deleting a channel
                case @"delete":
                    if (parts.Length < 2) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                        break;
                    }

                    if (parts[1].All(char.IsDigit))
                        goto case @"delmsg";
                    goto case @"delchan";

                // anyone can use these
                case @"join": // join a channel
                    if (parts.Length < 2)
                        break;

                    ChatChannel joinChan = Context.Channels.Get(parts[1]);

                    if (joinChan == null) {
                        user.Send(new LegacyCommandResponse(LCR.CHANNEL_NOT_FOUND, true, parts[1]));
                        user.ForceChannel();
                        break;
                    }

                    Context.SwitchChannel(user, joinChan, string.Join(' ', parts.Skip(2)));
                    break;
                case @"create": // create a new channel
                    if (user.Can(ChatUserPermissions.CreateChannel)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    bool createChanHasHierarchy;
                    if (parts.Length < 2 || (createChanHasHierarchy = parts[1].All(char.IsDigit) && parts.Length < 3)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                        break;
                    }

                    int createChanHierarchy = 0;
                    if (createChanHasHierarchy)
                        int.TryParse(parts[1], out createChanHierarchy);

                    if (createChanHierarchy > user.Hierarchy) {
                        user.Send(new LegacyCommandResponse(LCR.INSUFFICIENT_HIERARCHY));
                        break;
                    }

                    string createChanName = string.Join('_', parts.Skip(createChanHasHierarchy ? 2 : 1));
                    ChatChannel createChan = new ChatChannel {
                        Name = createChanName,
                        IsTemporary = !user.Can(ChatUserPermissions.SetChannelPermanent),
                        Hierarchy = createChanHierarchy,
                        Owner = user,
                    };

                    try {
                        Context.Channels.Add(createChan);
                    } catch (ChannelExistException) {
                        user.Send(new LegacyCommandResponse(LCR.CHANNEL_ALREADY_EXISTS, true, createChan.Name));
                        break;
                    } catch (ChannelInvalidNameException) {
                        user.Send(new LegacyCommandResponse(LCR.CHANNEL_NAME_INVALID));
                        break;
                    }

                    Context.SwitchChannel(user, createChan, createChan.Password);
                    user.Send(new LegacyCommandResponse(LCR.CHANNEL_CREATED, false, createChan.Name));
                    break;
                case @"delchan": // delete a channel
                    if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1])) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                        break;
                    }

                    string delChanName = string.Join('_', parts.Skip(1));
                    ChatChannel delChan = Context.Channels.Get(delChanName);

                    if (delChan == null) {
                        user.Send(new LegacyCommandResponse(LCR.CHANNEL_NOT_FOUND, true, delChanName));
                        break;
                    }

                    if (!user.Can(ChatUserPermissions.DeleteChannel) && delChan.Owner != user) {
                        user.Send(new LegacyCommandResponse(LCR.CHANNEL_DELETE_FAILED, true, delChan.Name));
                        break;
                    }

                    Context.Channels.Remove(delChan);
                    user.Send(new LegacyCommandResponse(LCR.CHANNEL_DELETED, false, delChan.Name));
                    break;
                case @"password": // set a password on the channel
                case @"pwd":
                    if (!user.Can(ChatUserPermissions.SetChannelPassword) || channel.Owner != user) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    string chanPass = string.Join(' ', parts.Skip(1)).Trim();

                    if (string.IsNullOrWhiteSpace(chanPass))
                        chanPass = string.Empty;

                    Context.Channels.Update(channel, password: chanPass);
                    user.Send(new LegacyCommandResponse(LCR.CHANNEL_PASSWORD_CHANGED, false));
                    break;
                case @"privilege": // sets a minimum hierarchy requirement on the channel
                case @"rank":
                case @"priv":
                    if (!user.Can(ChatUserPermissions.SetChannelHierarchy) || channel.Owner != user) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    if (parts.Length < 2 || !int.TryParse(parts[1], out int chanHierarchy) || chanHierarchy > user.Hierarchy) {
                        user.Send(new LegacyCommandResponse(LCR.INSUFFICIENT_HIERARCHY));
                        break;
                    }

                    Context.Channels.Update(channel, hierarchy: chanHierarchy);
                    user.Send(new LegacyCommandResponse(LCR.CHANNEL_HIERARCHY_CHANGED, false));
                    break;

                case @"say": // pretend to be the bot
                    if (!user.Can(ChatUserPermissions.Broadcast)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    Context.Send(new LegacyCommandResponse(LCR.BROADCAST, false, string.Join(' ', parts.Skip(1))));
                    break;
                case @"delmsg": // deletes a message
                    bool deleteAnyMessage = user.Can(ChatUserPermissions.DeleteAnyMessage);

                    if (!deleteAnyMessage && !user.Can(ChatUserPermissions.DeleteOwnMessage)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    if (parts.Length < 2 || !parts[1].All(char.IsDigit) || !int.TryParse(parts[1], out int delSeqId)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                        break;
                    }

                    IChatEvent delMsg = Context.Events.Get(delSeqId);

                    if (delMsg == null || delMsg.Sender.Hierarchy > user.Hierarchy || (!deleteAnyMessage && delMsg.Sender.UserId != user.UserId)) {
                        user.Send(new LegacyCommandResponse(LCR.MESSAGE_DELETE_ERROR));
                        break;
                    }

                    Context.Events.Remove(delMsg);
                    break;
                case @"kick": // kick a user from the server
                case @"ban": // ban a user from the server, this differs from /kick in that it adds all remote address to the ip banlist
                    bool isBanning = command == @"ban";

                    if (!user.Can(isBanning ? ChatUserPermissions.BanUser : ChatUserPermissions.KickUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    ChatUser banUser;

                    if (parts.Length < 2 || (banUser = Context.Users.Get(parts[1])) == null) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_FOUND, true, parts.Length < 2 ? @"User" : parts[1]));
                        break;
                    }

                    if (banUser == user || banUser.Hierarchy >= user.Hierarchy || Context.Bans.Check(banUser) > DateTimeOffset.Now) {
                        user.Send(new LegacyCommandResponse(LCR.KICK_NOT_ALLOWED, true, banUser.GetDisplayName(1)));
                        break;
                    }

                    DateTimeOffset? banUntil = isBanning ? (DateTimeOffset?)DateTimeOffset.MaxValue : null;

                    if (parts.Length > 2) {
                        if (!double.TryParse(parts[2], out double silenceSeconds)) {
                            user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                            break;
                        }

                        banUntil = DateTimeOffset.UtcNow.AddSeconds(silenceSeconds);
                    }

                    Context.BanUser(banUser, banUntil, isBanning);
                    break;
                case @"pardon":
                case @"unban":
                    if (!user.Can(ChatUserPermissions.BanUser | ChatUserPermissions.KickUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    if (parts.Length < 2) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_BANNED, true, string.Empty));
                        break;
                    }

                    BannedUser unbanUser = Context.Bans.GetUser(parts[1]);

                    if (unbanUser == null || unbanUser.Expires <= DateTimeOffset.Now) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_BANNED, true, unbanUser?.Username ?? parts[1]));
                        break;
                    }

                    Context.Bans.Remove(unbanUser);

                    user.Send(new LegacyCommandResponse(LCR.USER_UNBANNED, false, unbanUser));
                    break;
                case @"pardonip":
                case @"unbanip":
                    if (!user.Can(ChatUserPermissions.BanUser | ChatUserPermissions.KickUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    if (parts.Length < 2 || !IPAddress.TryParse(parts[1], out IPAddress unbanIP)) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_BANNED, true, string.Empty));
                        break;
                    }

                    if (Context.Bans.Check(unbanIP) <= DateTimeOffset.Now) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_BANNED, true, unbanIP));
                        break;
                    }

                    Context.Bans.Remove(unbanIP);

                    user.Send(new LegacyCommandResponse(LCR.USER_UNBANNED, false, unbanIP));
                    break;
                case @"bans": // gets a list of bans
                case @"banned":
                    if (!user.Can(ChatUserPermissions.BanUser | ChatUserPermissions.KickUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    user.Send(new BanListPacket(Context.Bans.All()));
                    break;
                case @"silence": // silence a user
                    if (!user.Can(ChatUserPermissions.SilenceUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    ChatUser silUser;

                    if (parts.Length < 2 || (silUser = Context.Users.Get(parts[1])) == null) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_FOUND, true, parts.Length < 2 ? @"User" : parts[1]));
                        break;
                    }

                    if (silUser == user) {
                        user.Send(new LegacyCommandResponse(LCR.SILENCE_SELF));
                        break;
                    }

                    if (silUser.Hierarchy >= user.Hierarchy) {
                        user.Send(new LegacyCommandResponse(LCR.SILENCE_HIERARCHY));
                        break;
                    }

                    if (silUser.IsSilenced) {
                        user.Send(new LegacyCommandResponse(LCR.SILENCE_ALREADY));
                        break;
                    }

                    DateTimeOffset silenceUntil = DateTimeOffset.MaxValue;

                    if (parts.Length > 2) {
                        if (!double.TryParse(parts[2], out double silenceSeconds)) {
                            user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                            break;
                        }

                        silenceUntil = DateTimeOffset.UtcNow.AddSeconds(silenceSeconds);
                    }

                    silUser.SilencedUntil = silenceUntil;
                    silUser.Send(new LegacyCommandResponse(LCR.SILENCED, false));
                    user.Send(new LegacyCommandResponse(LCR.TARGET_SILENCED, false, silUser.GetDisplayName(1)));
                    break;
                case @"unsilence": // unsilence a user
                    if (!user.Can(ChatUserPermissions.SilenceUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{command}"));
                        break;
                    }

                    ChatUser unsilUser;

                    if (parts.Length < 2 || (unsilUser = Context.Users.Get(parts[1])) == null) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_FOUND, true, parts.Length < 2 ? @"User" : parts[1]));
                        break;
                    }

                    if (unsilUser.Hierarchy >= user.Hierarchy) {
                        user.Send(new LegacyCommandResponse(LCR.UNSILENCE_HIERARCHY));
                        break;
                    }

                    if (!unsilUser.IsSilenced) {
                        user.Send(new LegacyCommandResponse(LCR.NOT_SILENCED));
                        break;
                    }

                    unsilUser.SilencedUntil = DateTimeOffset.MinValue;
                    unsilUser.Send(new LegacyCommandResponse(LCR.UNSILENCED, false));
                    user.Send(new LegacyCommandResponse(LCR.TARGET_UNSILENCED, false, unsilUser.GetDisplayName(1)));
                    break;
                case @"ip": // gets a user's ip (from all connections in this case)
                case @"whois":
                    if (!user.Can(ChatUserPermissions.SeeIPAddress)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, @"/ip"));
                        break;
                    }

                    ChatUser ipUser;
                    if (parts.Length < 2 || (ipUser = Context.Users.Get(parts[1])) == null) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_FOUND, true, parts.Length < 2 ? @"User" : parts[1]));
                        break;
                    }

                    foreach (IPAddress ip in ipUser.RemoteAddresses.Distinct().ToArray())
                        user.Send(new LegacyCommandResponse(LCR.IP_ADDRESS, false, ipUser.Username, ip));
                    break;

                default:
                    user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_FOUND, true, command));
                    break;
            }

            return null;
        }

        ~SockChatServer()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing) {
            if (IsDisposed)
                return;
            IsDisposed = true;

            Connections?.Clear();
            Server?.Dispose();
            Context?.Dispose();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
