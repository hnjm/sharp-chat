using SharpChat.Bans;
using SharpChat.Channels;
using SharpChat.Commands;
using SharpChat.Database;
using SharpChat.Events;
using SharpChat.Packets;
using SharpChat.Users;
using SharpChat.Users.Auth;
using SharpChat.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace SharpChat {
    public class SockChatServer : IDisposable {
        public const int EXT_VERSION = 2;
        public const int MSG_LENGTH_MAX = 5000;

#if DEBUG
        public const int MAX_CONNECTIONS = 9001;
        public const int FLOOD_KICK_LENGTH = 5;
        public const bool ENABLE_TYPING_EVENT = true;
#else
        public const int MAX_CONNECTIONS = 5;
        public const int FLOOD_KICK_LENGTH = 30;
        public const bool ENABLE_TYPING_EVENT = false;
#endif

        public static ChatUser Bot { get; } = new ChatUser {
            UserId = -1,
            Username = @"ChatBot",
            Rank = 0,
            Colour = new ChatColour(),
        };

        public IWebSocketServer Server { get; }
        public IDataProvider DataProvider { get; }
        public DatabaseWrapper Database { get; }
        public ChatContext Context { get; }

        public HttpClient HttpClient { get; }

        private IReadOnlyCollection<IChatCommand> Commands { get; } = new IChatCommand[] {
            new AFKCommand(),
            new WhisperCommand(),
            new ActionCommand(),

            new NickCommand(),

            new BroadcastCommand(),
        };

        public List<ChatUserSession> Sessions { get; } = new List<ChatUserSession>();
        private object SessionsLock { get; } = new object();

        public ChatUserSession GetSession(IWebSocketConnection conn) {
            lock(SessionsLock)
                return Sessions.FirstOrDefault(x => x.Connection == conn);
        }

        public SockChatServer(IWebSocketServer server, HttpClient httpClient, IDataProvider dataProvider, IDatabaseBackend databaseBackend) {
            Logger.Write("Starting Sock Chat server...");

            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Database = new DatabaseWrapper(databaseBackend ?? throw new ArgumentNullException(nameof(databaseBackend)));

            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            Context = new ChatContext(this);

            Context.Channels.Add(new ChatChannel(@"Lounge"));
#if DEBUG
            Context.Channels.Add(new ChatChannel(@"Programming"));
            Context.Channels.Add(new ChatChannel(@"Games"));
            Context.Channels.Add(new ChatChannel(@"Splatoon"));
            Context.Channels.Add(new ChatChannel(@"Password") { Password = @"meow", });
#endif
            Context.Channels.Add(new ChatChannel(@"Staff") { Rank = 5 });

            Server = server ?? throw new ArgumentNullException(nameof(server));
            Server.OnOpen += OnOpen;
            Server.OnClose += OnClose;
            Server.OnError += OnError;
            Server.OnMessage += OnMessage;
            Server.Start();
        }

        private void OnOpen(IWebSocketConnection conn) {
            lock(SessionsLock) {
                if(!Sessions.Any(x => x.Connection == conn))
                    Sessions.Add(new ChatUserSession(conn));
            }

            Context.Update();
        }

        private void OnClose(IWebSocketConnection conn) {
            ChatUserSession sess = GetSession(conn);

            // Remove connection from user
            if(sess?.User != null) {
                // RemoveConnection sets conn.User to null so we must grab a local copy.
                ChatUser user = sess.User;

                user.RemoveSession(sess);

                if(!user.HasSessions)
                    Context.UserLeave(null, user);
            }

            // Update context
            Context.Update();

            // Remove connection from server
            lock(SessionsLock)
                Sessions.Remove(sess);

            sess?.Dispose();
        }

        private void OnError(IWebSocketConnection conn, Exception ex) {
            ChatUserSession sess = GetSession(conn);
            string sessId = sess?.Id ?? new string('0', ChatUserSession.ID_LENGTH);
            Logger.Write($@"[{sessId} {conn.RemoteAddress}] {ex}");
            Context.Update();
        }

        private void OnMessage(IWebSocketConnection conn, string msg) {
            Context.Update();

            ChatUserSession sess = GetSession(conn);

            if(sess == null) {
                conn.Dispose();
                return;
            }

            if(sess.User is ChatUser && sess.User.HasFloodProtection) {
                sess.User.RateLimiter.AddTimePoint();

                if(sess.User.RateLimiter.State == ChatRateLimitState.Kick) {
                    Context.BanUser(sess.User, DateTimeOffset.UtcNow.AddSeconds(FLOOD_KICK_LENGTH), false, UserDisconnectReason.Flood);
                    return;
                } else if(sess.User.RateLimiter.State == ChatRateLimitState.Warning)
                    sess.User.Send(new FloodWarningPacket()); // make it so this thing only sends once
            }

            string[] args = msg.Split('\t');

            if(args.Length < 1 || !Enum.TryParse(args[0], out SockChatClientPacket opCode))
                return;

            switch(opCode) {
                case SockChatClientPacket.Ping:
                    if(!int.TryParse(args[1], out int pTime))
                        break;

                    sess.BumpPing();
                    sess.Send(new PongPacket(sess.LastPing));
                    break;

                case SockChatClientPacket.Authenticate:
                    if(sess.User != null)
                        break;

                    DateTimeOffset aBanned = Context.Bans.Check(sess.RemoteAddress);

                    if(aBanned > DateTimeOffset.UtcNow) {
                        sess.Send(new AuthFailPacket(AuthFailReason.Banned, aBanned));
                        sess.Dispose();
                        break;
                    }

                    if(args.Length < 3 || !long.TryParse(args[1], out long aUserId))
                        break;

                    IUserAuthResponse aAuthResponse;
                    try {
                        aAuthResponse = DataProvider.UserAuthClient.AttemptAuth(new UserAuthRequest(aUserId, args[2], sess.RemoteAddress));
                    } catch(Exception ex) {
                        Logger.Debug($@"<{sess.Id}> Auth fail: {ex.Message}");
                        sess.Send(new AuthFailPacket(AuthFailReason.AuthInvalid));
                        sess.Dispose();
                        return;
                    }
                     
                    ChatUser aUser = Context.Users.Get(aAuthResponse.UserId);

                    if(aUser == null)
                        aUser = new ChatUser(aAuthResponse);
                    else {
                        aUser.ApplyAuth(aAuthResponse);
                        aUser.Channel?.Send(new UserUpdatePacket(aUser));
                    }

                    aBanned = Context.Bans.Check(aUser);

                    if(aBanned > DateTimeOffset.Now) {
                        sess.Send(new AuthFailPacket(AuthFailReason.Banned, aBanned));
                        sess.Dispose();
                        return;
                    }

                    // Enforce a maximum amount of connections per user
                    if(aUser.SessionCount >= MAX_CONNECTIONS) {
                        sess.Send(new AuthFailPacket(AuthFailReason.MaxSessions));
                        sess.Dispose();
                        return;
                    }

                    // Bumping the ping to prevent upgrading
                    sess.BumpPing();

                    aUser.AddSession(sess);

                    sess.Send(new LegacyCommandResponse(LCR.WELCOME, false, $@"Welcome to Flashii Chat, {aUser.Username}!"));

                    if(File.Exists(@"welcome.txt")) {
                        IEnumerable<string> lines = File.ReadAllLines(@"welcome.txt").Where(x => !string.IsNullOrWhiteSpace(x));
                        string line = lines.ElementAtOrDefault(RNG.Next(lines.Count()));

                        if(!string.IsNullOrWhiteSpace(line))
                            sess.Send(new LegacyCommandResponse(LCR.WELCOME, false, line));
                    }

                    Context.HandleJoin(aUser, Context.Channels.DefaultChannel, sess);
                    break;

                case SockChatClientPacket.MessageSend:
                    if(args.Length < 3)
                        break;

                    ChatUser mUser = sess.User;

                    // No longer concats everything after index 1 with \t, no previous implementation did that either
                    string messageText = args.ElementAtOrDefault(2);

                    if(mUser == null || !mUser.Can(ChatUserPermissions.SendMessage) || string.IsNullOrWhiteSpace(messageText))
                        break;

#if !DEBUG
                    // Extra validation step, not necessary at all but enforces proper formatting in SCv1.
                    if (!long.TryParse(args[1], out long mUserId) || mUser.UserId != mUserId)
                        break;
#endif
                    ChatChannel mChannel = mUser.CurrentChannel;

                    if(mChannel == null
                        || !mUser.InChannel(mChannel)
                        || (mUser.IsSilenced && !mUser.Can(ChatUserPermissions.SilenceUser)))
                        break;

                    if(mUser.Status != ChatUserStatus.Online) {
                        mUser.Status = ChatUserStatus.Online;
                        mChannel.Send(new UserUpdatePacket(mUser));
                    }

                    if(messageText.Length > MSG_LENGTH_MAX)
                        messageText = messageText.Substring(0, MSG_LENGTH_MAX);

                    messageText = messageText.Trim();

#if DEBUG
                    Logger.Write($@"<{sess.Id} {mUser.Username}> {messageText}");
#endif

                    IChatMessageEvent message = null;

                    if(messageText[0] == '/') {
                        message = HandleV1Command(messageText, mUser, mChannel);

                        if(message == null)
                            break;
                    }

                    if(message == null)
                        message = new ChatMessageEvent(mUser, mChannel, messageText);

                    Context.Events.AddEvent(message);
                    mChannel.Send(new ChatMessageAddPacket(message));
                    break;

                case SockChatClientPacket.Typing:
                    if(!ENABLE_TYPING_EVENT || sess.User == null)
                        break;

                    ChatChannel tChannel = sess.User.CurrentChannel;
                    if(tChannel == null || !tChannel.CanType(sess.User))
                        break;

                    ChatChannelTyping tInfo = tChannel.RegisterTyping(sess.User);
                    if(tInfo == null)
                        return;

                    tChannel.Send(new TypingPacket(tChannel, tInfo));
                    break;
            }
        }

        public IChatMessageEvent HandleV1Command(string message, ChatUser user, ChatChannel channel) {
            string[] parts = message[1..].Split(' ');
            string commandName = parts[0].Replace(@".", string.Empty).ToLowerInvariant();

            for(int i = 1; i < parts.Length; i++)
                parts[i] = parts[i].Replace(@"<", @"&lt;")
                                   .Replace(@">", @"&gt;")
                                   .Replace("\n", @" <br/> ");

            IChatCommand command = null;
            foreach(IChatCommand cmd in Commands)
                if(cmd.IsMatch(commandName)) {
                    command = cmd;
                    break;
                }

            if(command != null) {
                try {
                    return command.Dispatch(new ChatCommandContext(parts, user, channel, Context));
                } catch(CommandException ex) {
                    user.Send(ex.ToPacket());
                    return null;
                }
            }

            switch(commandName) {
                case @"who": // gets all online users/online users in a channel if arg
                    StringBuilder whoChanSB = new StringBuilder();
                    string whoChanStr = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? parts[1] : string.Empty;

                    if(!string.IsNullOrEmpty(whoChanStr)) {
                        ChatChannel whoChan = Context.Channels.Get(whoChanStr);

                        if(whoChan == null) {
                            user.Send(new LegacyCommandResponse(LCR.CHANNEL_NOT_FOUND, true, whoChanStr));
                            break;
                        }

                        if(whoChan.Rank > user.Rank || (whoChan.HasPassword && !user.Can(ChatUserPermissions.JoinAnyChannel))) {
                            user.Send(new LegacyCommandResponse(LCR.USERS_LISTING_ERROR, true, whoChanStr));
                            break;
                        }

                        foreach(ChatUser whoUser in whoChan.GetUsers()) {
                            whoChanSB.Append(@"<a href=""javascript:void(0);"" onclick=""UI.InsertChatText(this.innerHTML);""");

                            if(whoUser == user)
                                whoChanSB.Append(@" style=""font-weight: bold;""");

                            whoChanSB.Append('>');
                            whoChanSB.Append(whoUser.DisplayName);
                            whoChanSB.Append(@"</a>, ");
                        }

                        if(whoChanSB.Length > 2)
                            whoChanSB.Length -= 2;

                        user.Send(new LegacyCommandResponse(LCR.USERS_LISTING_CHANNEL, false, whoChanSB));
                    } else {
                        foreach(ChatUser whoUser in Context.Users.All()) {
                            whoChanSB.Append(@"<a href=""javascript:void(0);"" onclick=""UI.InsertChatText(this.innerHTML);""");

                            if(whoUser == user)
                                whoChanSB.Append(@" style=""font-weight: bold;""");

                            whoChanSB.Append('>');
                            whoChanSB.Append(whoUser.DisplayName);
                            whoChanSB.Append(@"</a>, ");
                        }

                        if(whoChanSB.Length > 2)
                            whoChanSB.Length -= 2;

                        user.Send(new LegacyCommandResponse(LCR.USERS_LISTING_SERVER, false, whoChanSB));
                    }
                    break;

                // double alias for delchan and delmsg
                // if the argument is a number we're deleting a message
                // if the argument is a string we're deleting a channel
                case @"delete":
                    if(parts.Length < 2) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                        break;
                    }

                    if(parts[1].All(char.IsDigit))
                        goto case @"delmsg";
                    goto case @"delchan";

                // anyone can use these
                case @"join": // join a channel
                    if(parts.Length < 2)
                        break;

                    ChatChannel joinChan = Context.Channels.Get(parts[1]);

                    if(joinChan == null) {
                        user.Send(new LegacyCommandResponse(LCR.CHANNEL_NOT_FOUND, true, parts[1]));
                        user.ForceChannel();
                        break;
                    }

                    Context.SwitchChannel(user, joinChan, string.Join(' ', parts.Skip(2)));
                    break;
                case @"create": // create a new channel
                    if(user.Can(ChatUserPermissions.CreateChannel)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{commandName}"));
                        break;
                    }

                    bool createChanHasHierarchy;
                    if(parts.Length < 2 || (createChanHasHierarchy = parts[1].All(char.IsDigit) && parts.Length < 3)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                        break;
                    }

                    int createChanHierarchy = 0;
                    if(createChanHasHierarchy && !int.TryParse(parts[1], out createChanHierarchy))
                        createChanHierarchy = 0;

                    if(createChanHierarchy > user.Rank) {
                        user.Send(new LegacyCommandResponse(LCR.INSUFFICIENT_HIERARCHY));
                        break;
                    }

                    string createChanName = string.Join('_', parts.Skip(createChanHasHierarchy ? 2 : 1));
                    ChatChannel createChan = new ChatChannel {
                        Name = createChanName,
                        IsTemporary = !user.Can(ChatUserPermissions.SetChannelPermanent),
                        Rank = createChanHierarchy,
                        Owner = user,
                    };

                    try {
                        Context.Channels.Add(createChan);
                    } catch(ChannelExistException) {
                        user.Send(new LegacyCommandResponse(LCR.CHANNEL_ALREADY_EXISTS, true, createChan.Name));
                        break;
                    } catch(ChannelInvalidNameException) {
                        user.Send(new LegacyCommandResponse(LCR.CHANNEL_NAME_INVALID));
                        break;
                    }

                    Context.SwitchChannel(user, createChan, createChan.Password);
                    user.Send(new LegacyCommandResponse(LCR.CHANNEL_CREATED, false, createChan.Name));
                    break;
                case @"delchan": // delete a channel
                    if(parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1])) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                        break;
                    }

                    string delChanName = string.Join('_', parts.Skip(1));
                    ChatChannel delChan = Context.Channels.Get(delChanName);

                    if(delChan == null) {
                        user.Send(new LegacyCommandResponse(LCR.CHANNEL_NOT_FOUND, true, delChanName));
                        break;
                    }

                    if(!user.Can(ChatUserPermissions.DeleteChannel) && delChan.Owner != user) {
                        user.Send(new LegacyCommandResponse(LCR.CHANNEL_DELETE_FAILED, true, delChan.Name));
                        break;
                    }

                    Context.Channels.Remove(delChan);
                    user.Send(new LegacyCommandResponse(LCR.CHANNEL_DELETED, false, delChan.Name));
                    break;
                case @"password": // set a password on the channel
                case @"pwd":
                    if(!user.Can(ChatUserPermissions.SetChannelPassword) || channel.Owner != user) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{commandName}"));
                        break;
                    }

                    string chanPass = string.Join(' ', parts.Skip(1)).Trim();

                    if(string.IsNullOrWhiteSpace(chanPass))
                        chanPass = string.Empty;

                    Context.Channels.Update(channel, password: chanPass);
                    user.Send(new LegacyCommandResponse(LCR.CHANNEL_PASSWORD_CHANGED, false));
                    break;
                case @"privilege": // sets a minimum hierarchy requirement on the channel
                case @"rank":
                case @"priv":
                    if(!user.Can(ChatUserPermissions.SetChannelHierarchy) || channel.Owner != user) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{commandName}"));
                        break;
                    }

                    if(parts.Length < 2 || !int.TryParse(parts[1], out int chanHierarchy) || chanHierarchy > user.Rank) {
                        user.Send(new LegacyCommandResponse(LCR.INSUFFICIENT_HIERARCHY));
                        break;
                    }

                    Context.Channels.Update(channel, hierarchy: chanHierarchy);
                    user.Send(new LegacyCommandResponse(LCR.CHANNEL_HIERARCHY_CHANGED, false));
                    break;

                case @"delmsg": // deletes a message
                    bool deleteAnyMessage = user.Can(ChatUserPermissions.DeleteAnyMessage);

                    if(!deleteAnyMessage && !user.Can(ChatUserPermissions.DeleteOwnMessage)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{commandName}"));
                        break;
                    }

                    if(parts.Length < 2 || !parts[1].All(char.IsDigit) || !long.TryParse(parts[1], out long delSeqId)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                        break;
                    }

                    IChatEvent delMsg = Context.Events.GetEvent(delSeqId);

                    if(delMsg == null || delMsg.Sender.Rank > user.Rank || (!deleteAnyMessage && delMsg.Sender.UserId != user.UserId)) {
                        user.Send(new LegacyCommandResponse(LCR.MESSAGE_DELETE_ERROR));
                        break;
                    }

                    if(Context.Events.RemoveEvent(delMsg))
                        Context.Send(new ChatMessageDeletePacket(delMsg.SequenceId));
                    break;
                case @"kick": // kick a user from the server
                case @"ban": // ban a user from the server, this differs from /kick in that it adds all remote address to the ip banlist
                    bool isBanning = commandName == @"ban";

                    if(!user.Can(isBanning ? ChatUserPermissions.BanUser : ChatUserPermissions.KickUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{commandName}"));
                        break;
                    }

                    ChatUser banUser;

                    if(parts.Length < 2 || (banUser = Context.Users.Get(parts[1])) == null) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_FOUND, true, parts.Length < 2 ? @"User" : parts[1]));
                        break;
                    }

                    if(banUser == user || banUser.Rank >= user.Rank || Context.Bans.Check(banUser) > DateTimeOffset.Now) {
                        user.Send(new LegacyCommandResponse(LCR.KICK_NOT_ALLOWED, true, banUser.DisplayName));
                        break;
                    }

                    DateTimeOffset? banUntil = isBanning ? (DateTimeOffset?)DateTimeOffset.MaxValue : null;

                    if(parts.Length > 2) {
                        if(!double.TryParse(parts[2], out double silenceSeconds)) {
                            user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                            break;
                        }

                        banUntil = DateTimeOffset.UtcNow.AddSeconds(silenceSeconds);
                    }

                    Context.BanUser(banUser, banUntil, isBanning);
                    break;
                case @"pardon":
                case @"unban":
                    if(!user.Can(ChatUserPermissions.BanUser | ChatUserPermissions.KickUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{commandName}"));
                        break;
                    }

                    if(parts.Length < 2) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_BANNED, true, string.Empty));
                        break;
                    }

                    BannedUser unbanUser = Context.Bans.GetUser(parts[1]);

                    if(unbanUser == null || unbanUser.Expires <= DateTimeOffset.Now) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_BANNED, true, unbanUser?.Username ?? parts[1]));
                        break;
                    }

                    Context.Bans.Remove(unbanUser);

                    user.Send(new LegacyCommandResponse(LCR.USER_UNBANNED, false, unbanUser));
                    break;
                case @"pardonip":
                case @"unbanip":
                    if(!user.Can(ChatUserPermissions.BanUser | ChatUserPermissions.KickUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{commandName}"));
                        break;
                    }

                    if(parts.Length < 2 || !IPAddress.TryParse(parts[1], out IPAddress unbanIP)) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_BANNED, true, string.Empty));
                        break;
                    }

                    if(Context.Bans.Check(unbanIP) <= DateTimeOffset.Now) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_BANNED, true, unbanIP));
                        break;
                    }

                    Context.Bans.Remove(unbanIP);

                    user.Send(new LegacyCommandResponse(LCR.USER_UNBANNED, false, unbanIP));
                    break;
                case @"bans": // gets a list of bans
                case @"banned":
                    if(!user.Can(ChatUserPermissions.BanUser | ChatUserPermissions.KickUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{commandName}"));
                        break;
                    }

                    user.Send(new BanListPacket(Context.Bans.All()));
                    break;
                case @"silence": // silence a user
                    if(!user.Can(ChatUserPermissions.SilenceUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{commandName}"));
                        break;
                    }

                    ChatUser silUser;

                    if(parts.Length < 2 || (silUser = Context.Users.Get(parts[1])) == null) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_FOUND, true, parts.Length < 2 ? @"User" : parts[1]));
                        break;
                    }

                    if(silUser == user) {
                        user.Send(new LegacyCommandResponse(LCR.SILENCE_SELF));
                        break;
                    }

                    if(silUser.Rank >= user.Rank) {
                        user.Send(new LegacyCommandResponse(LCR.SILENCE_HIERARCHY));
                        break;
                    }

                    if(silUser.IsSilenced) {
                        user.Send(new LegacyCommandResponse(LCR.SILENCE_ALREADY));
                        break;
                    }

                    DateTimeOffset silenceUntil = DateTimeOffset.MaxValue;

                    if(parts.Length > 2) {
                        if(!double.TryParse(parts[2], out double silenceSeconds)) {
                            user.Send(new LegacyCommandResponse(LCR.COMMAND_FORMAT_ERROR));
                            break;
                        }

                        silenceUntil = DateTimeOffset.UtcNow.AddSeconds(silenceSeconds);
                    }

                    silUser.SilencedUntil = silenceUntil;
                    silUser.Send(new LegacyCommandResponse(LCR.SILENCED, false));
                    user.Send(new LegacyCommandResponse(LCR.TARGET_SILENCED, false, silUser.DisplayName));
                    break;
                case @"unsilence": // unsilence a user
                    if(!user.Can(ChatUserPermissions.SilenceUser)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, $@"/{commandName}"));
                        break;
                    }

                    ChatUser unsilUser;

                    if(parts.Length < 2 || (unsilUser = Context.Users.Get(parts[1])) == null) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_FOUND, true, parts.Length < 2 ? @"User" : parts[1]));
                        break;
                    }

                    if(unsilUser.Rank >= user.Rank) {
                        user.Send(new LegacyCommandResponse(LCR.UNSILENCE_HIERARCHY));
                        break;
                    }

                    if(!unsilUser.IsSilenced) {
                        user.Send(new LegacyCommandResponse(LCR.NOT_SILENCED));
                        break;
                    }

                    unsilUser.SilencedUntil = DateTimeOffset.MinValue;
                    unsilUser.Send(new LegacyCommandResponse(LCR.UNSILENCED, false));
                    user.Send(new LegacyCommandResponse(LCR.TARGET_UNSILENCED, false, unsilUser.DisplayName));
                    break;
                case @"ip": // gets a user's ip (from all connections in this case)
                case @"whois":
                    if(!user.Can(ChatUserPermissions.SeeIPAddress)) {
                        user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_ALLOWED, true, @"/ip"));
                        break;
                    }

                    ChatUser ipUser;
                    if(parts.Length < 2 || (ipUser = Context.Users.Get(parts[1])) == null) {
                        user.Send(new LegacyCommandResponse(LCR.USER_NOT_FOUND, true, parts.Length < 2 ? @"User" : parts[1]));
                        break;
                    }

                    foreach(IPAddress ip in ipUser.RemoteAddresses.Distinct().ToArray())
                        user.Send(new LegacyCommandResponse(LCR.IP_ADDRESS, false, ipUser.Username, ip));
                    break;

                default:
                    user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_FOUND, true, commandName));
                    break;
            }

            return null;
        }

        private bool IsDisposed;

        ~SockChatServer()
            => DoDispose();

        public void Dispose() { 
            DoDispose();
            GC.SuppressFinalize(this);
        }

        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;

            Sessions?.Clear();
            Server?.Dispose();
            Context?.Dispose();
            HttpClient?.Dispose();
            Database?.Dispose();
        }
    }
}
