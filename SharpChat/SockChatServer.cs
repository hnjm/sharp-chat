using Fleck;
using SharpChat.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpChat
{
    public class SockChatServer : IDisposable
    {
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

        public bool IsDisposed { get; private set; }

        public static readonly SockChatUser Bot = new SockChatUser
        {
            UserId = -1,
            Username = @"ChatBot",
            Hierarchy = 0,
            Colour = new FlashiiColour(),
        };

        public readonly WebSocketServer Server;
        public readonly SockChatContext Context;

        public readonly List<SockChatConn> Connections = new List<SockChatConn>();

        public SockChatServer(ushort port)
        {
            Logger.Write("Starting Sock Chat server...");

            Context = new SockChatContext(this);

            Context.AddChannel(new SockChatChannel(@"Lounge"));
#if DEBUG
            Context.AddChannel(new SockChatChannel(@"Programming"));
            Context.AddChannel(new SockChatChannel(@"Games"));
            Context.AddChannel(new SockChatChannel(@"Splatoon"));
            Context.AddChannel(new SockChatChannel(@"Password") { Password = @"meow", });
#endif
            Context.AddChannel(new SockChatChannel(@"Staff") { Hierarchy = 5 });

            Server = new WebSocketServer($@"ws://0.0.0.0:{port}");
            Server.Start(sock =>
            {
                sock.OnOpen = () => OnOpen(sock);
                sock.OnClose = () => OnClose(sock);
                sock.OnError = err => OnError(sock, err);
                sock.OnMessage = msg => OnMessage(sock, msg);
            });
        }

        private void OnOpen(IWebSocketConnection conn)
        {
            lock(Connections)
            {
                SockChatConn sConn = Connections.FirstOrDefault(x => x.Websocket == conn);

                if (sConn == null)
                    Connections.Add(sConn = new SockChatConn(conn));
            }

            Context.CheckIPBanExpirations();
            Context.CheckPings();
        }

        private void OnClose(IWebSocketConnection conn)
        {
            lock (Context.Users)
            {
                SockChatUser user = Context.FindUserBySock(conn);

                if (user != null)
                {
                    user.RemoveConnection(conn);

                    if (!user.Connections.Any())
                        Context.UserLeave(null, user);
                }
            }

            Context.CheckIPBanExpirations();
            Context.CheckPings();

            lock (Connections)
            {
                SockChatConn sConn = Connections.FirstOrDefault(x => x.Websocket == conn);

                if (sConn != null)
                {
                    Connections.Remove(sConn);
                    sConn.Dispose();
                }
            }
        }

        private void OnError(IWebSocketConnection conn, Exception ex)
        {
            Logger.Write($@"[{conn.ConnectionInfo.ClientIpAddress}] {ex}");
            Context.CheckIPBanExpirations();
            Context.CheckPings();
        }

        private void OnMessage(IWebSocketConnection ws, string msg)
        {
            Context.CheckIPBanExpirations();
            Context.CheckPings();

            SockChatConn conn;

            lock (Connections)
                conn = Connections.FirstOrDefault(x => x.Websocket == ws);

            if (conn == null)
            {
                Logger.Write(@"Somehow got to OnMessage without a valid SockChatConn.");
                ws.Close();
                return;
            }

            SockChatUser floodUser = Context.FindUserBySock(conn);

            if(floodUser != null)
            {
                floodUser.RateLimiter.AddTimePoint();

                if(floodUser.RateLimiter.State == ChatRateLimitState.Kick)
                {
                    Context.BanUser(floodUser, DateTimeOffset.UtcNow.AddSeconds(30), false, Constants.LEAVE_FLOOD);
                    return;
                } else if(floodUser.RateLimiter.State == ChatRateLimitState.Warning)
                    floodUser.Send(false, @"flwarn");
            }

            string[] args = msg.Split('\t');

            if (args.Length < 1 || !Enum.TryParse(args[0], out SockChatClientPacket opCode))
                return;

            switch (opCode)
            {
                case SockChatClientPacket.Ping:
                    if (!int.TryParse(args[1], out int pTime))
                        break;

                    conn.BumpPing();
                    conn.Send(new PongPacket(conn.LastPing));
                    break;

                case SockChatClientPacket.Upgrade:
#pragma warning disable CS0162
                    if (conn.LastPing != DateTimeOffset.MinValue || VERSION < 2)
                        break;
#pragma warning restore CS0162

                    if (!int.TryParse(args[1], out int uVersion) || uVersion < 2 || uVersion > VERSION)
                    {
                        conn.Send(new UpgradeAckPacket(false, VERSION));
                        break;
                    }

                    conn.Version = uVersion;
                    conn.Send(new UpgradeAckPacket(true, conn.Version));
                    conn.BumpPing();
                    break;

                case SockChatClientPacket.Authenticate:
                    if (Context.FindUserBySock(conn) != null)
                        break;

                    DateTimeOffset authBan = Context.GetIPBanExpiration(conn.RemoteAddress);

                    if (authBan > DateTimeOffset.UtcNow)
                    {
                        conn.Send(new AuthFailPacket(AuthFailReason.Banned, authBan));
                        conn.Dispose();
                        break;
                    }

                    SockChatUser aUser;
                    FlashiiAuth auth;

                    aUser = Context.FindUserBySock(conn);

                    if (aUser != null || args.Length < 3 || !int.TryParse(args[1], out int aUserId))
                        break;

                    auth = FlashiiAuth.Attempt(aUserId, args[2], conn.RemoteAddress);

                    if (!auth.Success)
                    {
                        conn.Send(new AuthFailPacket(AuthFailReason.AuthInvalid));
                        conn.Dispose();
                        break;
                    }

                    aUser = Context.FindUserById(auth.UserId);

                    if (aUser == null)
                        aUser = new SockChatUser(auth);
                    else
                    {
                        aUser.ApplyAuth(auth);
                        aUser.Channel?.Send(new UserUpdatePacket(aUser));
                    }

                    if (aUser.IsBanned)
                    {
                        conn.Send(new AuthFailPacket(AuthFailReason.Banned, aUser.BannedUntil));
                        conn.Dispose();
                        break;
                    }

                    // arbitrarily limit users to five connections
                    if (aUser.Connections.Count >= MAX_CONNECTIONS)
                    {
                        conn.Send(new AuthFailPacket(AuthFailReason.MaxSessions));
                        conn.Dispose();
                        break;
                    }

                    // Bumping the ping to prevent upgrading
                    conn.BumpPing();

                    aUser.AddConnection(conn);

                    SockChatChannel chan = Context.FindChannelByName(auth.DefaultChannel) ?? Context.Channels.FirstOrDefault();

                    // umi eats the first message for some reason so we'll send a blank padding msg
                    conn.Send(new ContextMessagePacket(EventChatMessage.Info(@"welcome", SockChatMessageFlags.RegularUser, @"say", Utils.InitialMessage)));
                    conn.Send(new ContextMessagePacket(EventChatMessage.Info(@"welcome", SockChatMessageFlags.RegularUser, @"say", $@"Welcome to the temporary drop in chat, {aUser.Username}!")));

                    Context.HandleJoin(aUser, chan, conn);
                    break;

                case SockChatClientPacket.MessageSend:
                    if (args.Length < 3)
                        break;

                    lock (Context)
                    {
                        SockChatUser mUser = Context.FindUserBySock(conn);
                        SockChatChannel mChan;

                        if (mUser == null || string.IsNullOrWhiteSpace(args[2]))
                            break;

                        if (conn.Version < 2)
                        {
#if !DEBUG
                            if (!int.TryParse(args[1], out int mUserId) || mUser.UserId != mUserId)
                                break;
#endif
                            mChan = Context.FindUserChannel(mUser);
                        }
                        else
                            mChan = Context.FindChannelByName(args[1]);

                        if (mChan == null || !mUser.Channels.Contains(mChan) || (mUser.IsSilenced && !mUser.IsModerator))
                            break;

                        if (mUser.IsAway)
                        {
                            mUser.AwayMessage = null;
                            mChan.Send(new UserUpdatePacket(mUser));
                        }

                        string message = string.Join('\t', args.Skip(2)).Trim();

#if DEBUG
                        Logger.Write($@"<{mUser.Username}> {message}");
#endif

                        if (message.Length > 2000)
                            message = message.Substring(0, 2000);

                        // These commands are only available in V1, all server side commands are to be replaced with packets and client side commands.
                        if (conn.Version < 2 && message[0] == '/')
                        {
                            string[] parts = message.Substring(1).Split(' ');
                            string command = parts[0].Replace(@".", string.Empty).ToLowerInvariant();

                            for (int i = 1; i < parts.Length; i++)
                                parts[i] = parts[i].SanitiseMessage();

                            switch (command)
                            {
                                case @"afk": // go afk
                                    string afkStr = parts.Length < 2 || string.IsNullOrEmpty(parts[1])
                                        ? @"AFK"
                                        : string.Join(' ', parts.Skip(1));

                                    if (!string.IsNullOrEmpty(afkStr))
                                    {
                                        mUser.AwayMessage = afkStr.Substring(0, Math.Min(afkStr.Length, 100)).Trim();
                                        mChan.Send(new UserUpdatePacket(mUser));
                                    }
                                    break;
                                case @"nick": // sets a temporary nickname
                                    if (!mUser.CanChangeNick)
                                    {
                                        mUser.Send(true, @"cmdna", @"/nick");
                                        break;
                                    }

                                    string nickStr = string.Join('_', parts.Skip(1)).Trim().SanitiseUsername();

                                    if (nickStr == mUser.Username)
                                        nickStr = null;
                                    else if (nickStr.Length > 15)
                                        nickStr = nickStr.Substring(0, 15);
                                    else if (string.IsNullOrEmpty(nickStr))
                                        nickStr = null;

                                    if (nickStr != null && Context.FindUserByName(nickStr) != null)
                                    {
                                        mUser.Send(true, @"nameinuse", nickStr);
                                        break;
                                    }

                                    string previousName = mUser.Nickname ?? mUser.Username;
                                    mUser.Nickname = nickStr;
                                    mChan.Send(new UserUpdatePacket(mUser, previousName));
                                    break;
                                case @"whisper": // sends a pm to another user
                                case @"msg":
                                    if (parts.Length < 3)
                                    {
                                        mUser.Send(true, @"cmderr");
                                        break;
                                    }

                                    SockChatUser whisperUser = Context.FindUserByName(parts[1]);

                                    if (whisperUser == null)
                                    {
                                        mUser.Send(true, @"usernf", parts[1]);
                                        break;
                                    }

                                    string whisperStr = string.Join(' ', parts.Skip(2));

                                    whisperUser.Send(mUser, whisperStr, SockChatMessageFlags.RegularPM);
                                    mUser.Send(mUser, $@"{whisperUser.DisplayName} {whisperStr}", SockChatMessageFlags.RegularPM);
                                    break;
                                case @"action": // describe an action
                                case @"me":
                                    if (parts.Length < 2)
                                        break;

                                    string actionMsg = string.Join(' ', parts.Skip(1));

                                    lock (Lock)
                                    {
                                        SockChatMessage sMsg = new SockChatMessage
                                        {
                                            MessageId = SockChatMessage.NextMessageId,
                                            Channel = mChan,
                                            DateTime = DateTimeOffset.UtcNow,
                                            User = mUser,
                                            Text = actionMsg,
                                            Flags = SockChatMessageFlags.Action,
                                        };

                                        Context.Messages.Add(sMsg);
                                        mChan.Send(new ChatMessageAddPacket(sMsg), sMsg.MessageId);
                                    }
                                    break;
                                case @"who": // gets all online users/online users in a channel if arg
                                    StringBuilder whoChanSB = new StringBuilder();
                                    string whoChanStr = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? parts[1] : string.Empty;

                                    if (!string.IsNullOrEmpty(whoChanStr))
                                    {
                                        SockChatChannel whoChan = Context.FindChannelByName(whoChanStr);

                                        if (whoChan == null)
                                        {
                                            mUser.Send(true, @"nochan", whoChanStr);
                                            break;
                                        }

                                        if (whoChan.Hierarchy > mUser.Hierarchy || (whoChan.HasPassword && !mUser.IsModerator))
                                        {
                                            mUser.Send(true, @"whoerr", whoChanStr);
                                            break;
                                        }

                                        lock (whoChan.Users)
                                            whoChan.GetUsers().ForEach(u => {
                                                whoChanSB.Append(@"<a href=""javascript:void(0);"" onclick=""UI.InsertChatText(this.innerHTML);""");

                                                if (u == mUser)
                                                    whoChanSB.Append(@" style=""font-weight: bold;""");

                                                whoChanSB.Append(@">");
                                                whoChanSB.Append(u.DisplayName);
                                                whoChanSB.Append(@"</a>, ");
                                            });

                                        if (whoChanSB.Length > 2)
                                            whoChanSB.Length -= 2;

                                        mUser.Send(false, @"whochan", whoChan.Name, whoChanSB.ToString());
                                    }
                                    else
                                    {
                                        lock (Context.Users)
                                            Context.Users.ForEach(u => {
                                                whoChanSB.Append(@"<a href=""javascript:void(0);"" onclick=""UI.InsertChatText(this.innerHTML);""");

                                                if (u == mUser)
                                                    whoChanSB.Append(@" style=""font-weight: bold;""");

                                                whoChanSB.Append(@">");
                                                whoChanSB.Append(u.DisplayName);
                                                whoChanSB.Append(@"</a>, ");
                                            });

                                        if (whoChanSB.Length > 2)
                                            whoChanSB.Length -= 2;

                                        mUser.Send(false, @"who", whoChanSB.ToString());
                                    }
                                    break;

                                // double alias for delchan and delmsg
                                // if the argument is a number we're deleting a message
                                // if the argument is a string we're deleting a channel
                                case @"delete":
                                    if (parts.Length < 2)
                                    {
                                        mUser.Send(true, @"cmderr");
                                        break;
                                    }

                                    if (parts[1].IsNumeric())
                                        goto case @"delmsg";
                                    goto case @"delchan";

                                // anyone can use these
                                case @"join": // join a channel
                                    if (parts.Length < 2)
                                        break;

                                    Context.SwitchChannel(mUser, parts[1], string.Join(' ', parts.Skip(2)));
                                    break;
                                case @"create": // create a new channel
                                    if (mUser.CanCreateChannels == SockChatUserChannel.No)
                                    {
                                        mUser.Send(true, @"cmdna", @"/create");
                                        break;
                                    }

                                    bool createChanHasHierarchy;
                                    if (parts.Length < 2 || (createChanHasHierarchy = parts[1].IsNumeric() && parts.Length < 3))
                                    {
                                        mUser.Send(true, @"cmderr");
                                        break;
                                    }

                                    int createChanHierarchy = 0;
                                    if (createChanHasHierarchy)
                                        int.TryParse(parts[1], out createChanHierarchy);

                                    if (createChanHierarchy > mUser.Hierarchy)
                                    {
                                        mUser.Send(true, @"rankerr");
                                        break;
                                    }

                                    string createChanName = string.Join('_', parts.Skip(createChanHasHierarchy ? 2 : 1));
                                    SockChatChannel createChan = new SockChatChannel
                                    {
                                        Name = createChanName,
                                        IsTemporary = mUser.CanCreateChannels == SockChatUserChannel.OnlyTemporary,
                                        Hierarchy = createChanHierarchy,
                                        Owner = mUser,
                                    };

                                    string createChanResult = Context.AddChannel(createChan);

                                    if (!string.IsNullOrEmpty(createChanResult))
                                    {
                                        mUser.Send(Bot, createChanResult);
                                        break;
                                    }

                                    Context.SwitchChannel(mUser, createChan, createChan.Password);
                                    mUser.Send(false, @"crchan", createChan.Name);
                                    break;
                                case @"delchan": // delete a channel
                                    if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
                                    {
                                        mUser.Send(true, @"cmderr");
                                        break;
                                    }

                                    string delChanName = string.Join('_', parts.Skip(1));
                                    SockChatChannel delChan = Context.FindChannelByName(delChanName);

                                    if (delChan == null)
                                    {
                                        mUser.Send(true, @"nochan", delChanName);
                                        break;
                                    }

                                    if (!mUser.IsModerator && delChan.Owner != mUser)
                                    {
                                        mUser.Send(true, @"ndchan", delChan.Name);
                                        break;
                                    }

                                    Context.DeleteChannel(delChan);
                                    mUser.Send(false, @"delchan", delChan.Name);
                                    break;
                                case @"password": // set a password on the channel
                                case @"pwd":
                                    if (!mUser.IsModerator || mChan.Owner != mUser)
                                    {
                                        mUser.Send(true, @"cmdna", @"/pwd");
                                        break;
                                    }

                                    mChan.Password = string.Join(' ', parts.Skip(1)).Trim();

                                    if (string.IsNullOrEmpty(mChan.Password))
                                        mChan.Password = null;

                                    Context.UpdateChannel(mChan);
                                    mUser.Send(false, @"cpwdchan");
                                    break;
                                case @"privilege": // sets a minimum hierarchy requirement on the channel
                                case @"rank":
                                case @"priv":
                                    if (!mUser.IsModerator || mChan.Owner != mUser)
                                    {
                                        mUser.Send(true, @"cmdna", @"/priv");
                                        break;
                                    }

                                    if (parts.Length < 2 || !int.TryParse(parts[1], out int chanHierarchy) || chanHierarchy > mUser.Hierarchy)
                                    {
                                        mUser.Send(true, @"rankerr");
                                        break;
                                    }

                                    mChan.Hierarchy = chanHierarchy;
                                    Context.UpdateChannel(mChan);
                                    mUser.Send(false, @"cprivchan");
                                    break;

                                case @"say": // pretend to be the bot
                                    if (!mUser.IsModerator)
                                    {
                                        mUser.Send(true, @"cmdna", @"/say");
                                        break;
                                    }

                                    lock (Lock)
                                    {
                                        SockChatMessage sMsg = new SockChatMessage
                                        {
                                            MessageId = SockChatMessage.NextMessageId,
                                            Channel = mChan,
                                            DateTime = DateTimeOffset.UtcNow,
                                            User = Bot,
                                            Text = string.Join(' ', parts.Skip(1)),
                                        };

                                        Context.Messages.Add(sMsg);
                                        mChan.Send(new ChatMessageAddPacket(sMsg), sMsg.MessageId);
                                    }

                                    Context.Broadcast(Bot, SockChatMessage.PackBotMessage(0, @"say", string.Join(' ', parts.Skip(1))));
                                    break;
                                case @"delmsg": // deletes a message
                                    if (!mUser.IsModerator)
                                    {
                                        mUser.Send(true, @"cmdna", @"/delmsg");
                                        break;
                                    }

                                    if (parts.Length < 2 || !parts[1].IsNumeric() || !int.TryParse(parts[1], out int delMsgId))
                                    {
                                        mUser.Send(true, @"cmderr");
                                        break;
                                    }

                                    IChatMessage delMsg = Context.Messages.FirstOrDefault(m => m.MessageId == delMsgId);

                                    if (delMsg == null || delMsg.User.Hierarchy > mUser.Hierarchy)
                                    {
                                        mUser.Send(true, @"delerr");
                                        break;
                                    }

                                    Context.DeleteMessage(delMsg);
                                    break;
                                case @"kick": // kick a user from the server
                                case @"ban": // ban a user from the server, this differs from /kick in that it adds all remote address to the ip banlist
                                    if (!mUser.IsModerator)
                                    {
                                        mUser.Send(true, @"cmdna", $@"/{command}");
                                        break;
                                    }

                                    bool isBanning = command == @"ban";

                                    SockChatUser banUser;
                                    if (parts.Length < 2 || (banUser = Context.FindUserByName(parts[1])) == null)
                                    {
                                        mUser.Send(true, @"usernf", parts[1] ?? @"User");
                                        break;
                                    }

                                    if (banUser == mUser || banUser.Hierarchy >= mUser.Hierarchy || banUser.IsBanned)
                                    {
                                        mUser.Send(true, @"kickna", banUser.DisplayName);
                                        break;
                                    }

                                    DateTimeOffset? banUntil = isBanning ? (DateTimeOffset?)DateTimeOffset.MaxValue : null;

                                    if (parts.Length > 2)
                                    {
                                        if (!double.TryParse(parts[2], out double silenceSeconds))
                                        {
                                            mUser.Send(true, @"cmderr");
                                            break;
                                        }

                                        banUntil = DateTimeOffset.UtcNow.AddSeconds(silenceSeconds);
                                    }

                                    Context.BanUser(banUser, banUntil, isBanning);
                                    break;
                                case @"pardon": // unban a user
                                case @"unban":
                                    if (!mUser.IsModerator)
                                    {
                                        mUser.Send(true, @"cmdna", @"/unban");
                                        break;
                                    }

                                    if(parts.Length < 2)
                                    {
                                        mUser.Send(true, @"notban", @"User");
                                        break;
                                    }

                                    SockChatUser unbanUser = Context.FindUserByName(parts[1]);

                                    if(unbanUser == null || !unbanUser.IsBanned)
                                    {
                                        mUser.Send(true, @"notban", unbanUser?.DisplayName ?? parts[1]);
                                        break;
                                    }

                                    unbanUser.BannedUntil = DateTimeOffset.MinValue;
                                    mUser.Send(false, @"unban", unbanUser.Username);
                                    break;
                                case @"pardonip": // unban an ip
                                case @"unbanip":
                                    if (!mUser.IsModerator)
                                    {
                                        mUser.Send(true, @"cmdna", @"/unbanip");
                                        break;
                                    }

                                    if (parts.Length < 2 || !IPAddress.TryParse(parts[1], out IPAddress unbanIP))
                                    {
                                        mUser.Send(true, @"notban", @"0.0.0.0");
                                        break;
                                    }

                                    if (!Context.CheckIPBan(unbanIP))
                                    {
                                        mUser.Send(true, @"notban", unbanIP.ToString());
                                        break;
                                    }

                                    Context.IPBans.Remove(unbanIP);
                                    mUser.Send(false, @"unban", unbanIP.ToString());
                                    break;
                                case @"bans": // gets a list of bans
                                case @"banned":
                                    if (!mUser.IsModerator)
                                    {
                                        mUser.Send(true, @"cmdna", @"/bans");
                                        break;
                                    }

                                    mUser.Send(new BanListPacket(Context.Users.Where(u => u.IsBanned), Context.IPBans));
                                    break;
                                case @"silence": // silence a user
                                    if (!mUser.IsModerator)
                                    {
                                        mUser.Send(true, @"cmdna", @"/silence");
                                        break;
                                    }

                                    SockChatUser silUser;
                                    if (parts.Length < 2 || (silUser = Context.FindUserByName(parts[1])) == null)
                                    {
                                        mUser.Send(true, @"usernf", parts[1] ?? @"User");
                                        break;
                                    }

                                    if (silUser == mUser)
                                    {
                                        mUser.Send(true, @"silself");
                                        break;
                                    }

                                    if (silUser.Hierarchy >= mUser.Hierarchy)
                                    {
                                        mUser.Send(true, @"silperr");
                                        break;
                                    }

                                    if (silUser.IsSilenced)
                                    {
                                        mUser.Send(true, @"silerr");
                                        break;
                                    }

                                    DateTimeOffset silenceUntil = DateTimeOffset.MaxValue;

                                    if (parts.Length > 2)
                                    {
                                        if(!double.TryParse(parts[2], out double silenceSeconds))
                                        {
                                            mUser.Send(true, @"cmderr");
                                            break;
                                        }

                                        silenceUntil = DateTimeOffset.UtcNow.AddSeconds(silenceSeconds);
                                    }

                                    silUser.SilencedUntil = silenceUntil;
                                    silUser.Send(false, @"silence");
                                    mUser.Send(false, @"silok", silUser.DisplayName);
                                    break;
                                case @"unsilence": // unsilence a user
                                    if (!mUser.IsModerator)
                                    {
                                        mUser.Send(true, @"cmdna", @"/silence");
                                        break;
                                    }

                                    SockChatUser unsilUser;
                                    if(parts.Length < 2 || (unsilUser = Context.FindUserByName(parts[1])) == null)
                                    {
                                        mUser.Send(true, @"usernf", parts[1] ?? @"User");
                                        break;
                                    }

                                    if(unsilUser.Hierarchy >= mUser.Hierarchy)
                                    {
                                        mUser.Send(true, @"usilperr");
                                        break;
                                    }

                                    if (!unsilUser.IsSilenced)
                                    {
                                        mUser.Send(true, @"usilerr");
                                        break;
                                    }

                                    unsilUser.SilencedUntil = DateTimeOffset.MinValue;
                                    unsilUser.Send(false, @"unsil");
                                    mUser.Send(false, @"usilok", unsilUser.DisplayName);
                                    break;
                                case @"ip": // gets a user's ip (from all connections in this case)
                                case @"whois":
                                    if (!mUser.IsModerator)
                                    {
                                        mUser.Send(true, @"cmdna", @"/ip");
                                        break;
                                    }

                                    SockChatUser ipUser;
                                    if (parts.Length < 2 || (ipUser = Context.FindUserByName(parts[1])) == null)
                                    {
                                        mUser.Send(true, @"usernf", parts[1] ?? string.Empty);
                                        break;
                                    }

                                    ipUser.RemoteAddresses.Distinct().ForEach(
                                        ip => mUser.Send(false, @"ipaddr", ipUser.Username, ip)
                                    );
                                    break;

                                default:
                                    mUser.Send(true, @"nocmd", command);
                                    break;
                            }
                            break;
                        }

                        lock (Lock)
                        {
                            SockChatMessage sMsg = new SockChatMessage
                            {
                                MessageId = SockChatMessage.NextMessageId,
                                Channel = mChan,
                                DateTime = DateTimeOffset.UtcNow,
                                User = mUser,
                                Text = message,
                            };

                            Context.Messages.Add(sMsg);
                            mChan.Send(new ChatMessageAddPacket(sMsg), sMsg.MessageId);
                        }
                    }
                    break;
            }
        }

        public readonly object Lock = new object();

        ~SockChatServer()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            IsDisposed = true;

            Server?.Dispose();
            Context?.Dispose();

            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
