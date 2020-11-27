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
using System.Net.Http;

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
            new JoinCommand(),
            new AFKCommand(),
            new WhisperCommand(),
            new ActionCommand(),
            new WhoCommand(),
            new DeleteMessageCommand(),

            new NickCommand(),
            new CreateChannelCommand(),
            new DeleteChannelCommand(),
            new ChannelPasswordCommand(),
            new ChannelRankCommand(),

            new BroadcastCommand(),
            new KickBanUserCommand(),
            new PardonUserCommand(),
            new PardonIPCommand(),
            new BanListCommand(),
            new WhoIsUserCommand(),
            new SilenceUserCommand(),
            new UnsilenceUserCommand(),
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
                        message = HandleCommand(messageText, mUser, mChannel);

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

        public IChatMessageEvent HandleCommand(string message, ChatUser user, ChatChannel channel) {
            string[] parts = message[1..].Split(' ');
            string commandName = parts[0].Replace(@".", string.Empty).ToLowerInvariant();

            for(int i = 1; i < parts.Length; i++)
                parts[i] = parts[i].Replace(@"<", @"&lt;")
                                   .Replace(@">", @"&gt;")
                                   .Replace("\n", @" <br/> ");

            IChatCommand command = Commands.FirstOrDefault(x => x.IsCommandMatch(commandName, parts));
            if(command == null)
                user.Send(new LegacyCommandResponse(LCR.COMMAND_NOT_FOUND, true, commandName));

            try {
                return command.DispatchCommand(new ChatCommandContext(parts, user, channel, Context));
            } catch(CommandException ex) {
                user.Send(ex.ToPacket());
                return null;
            }
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
