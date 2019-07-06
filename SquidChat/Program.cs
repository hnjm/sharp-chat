using Fleck;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace SquidChat
{
    public class Program
    {
        public readonly static List<SockChatUser> Users = new List<SockChatUser>();
        public readonly static List<SockChatChannel> Channels = new List<SockChatChannel> { new SockChatChannel { Name = @"Lounge" } };
        public readonly static List<SockChatMessage> Messages = new List<SockChatMessage> { new SockChatMessage { Channel = null, DateTime = DateTimeOffset.UtcNow, MessageId = 1, Text = @"boob", User = new SockChatUser { UserId = 5, Username = @"Meoww", Colour = @"#09f", Hierarchy = -1 } } };

        public static int MessageId { get; private set; } = 0;

        public static int NextMessageId => ++MessageId;

        public static readonly SockChatUser Bot = new SockChatUser {
            UserId = -1,
            Username = @"ChatBot",
            Hierarchy = 0,
            Colour = @"inherit",
        };

        public static void Main(string[] args)
        {
            Console.WriteLine("SquidChat - Multi-user (PHP) Sock Chat");

            WebSocketServer srv = new WebSocketServer("ws://0.0.0.0:6770");
            srv.Start(s =>
            {
                s.OnOpen = () => OnOpen(s);
                s.OnClose = () => OnClose(s);
                s.OnError = err => OnError(s, err);
                s.OnMessage = msg => OnMessage(s, msg);
            });
            Console.ReadLine();
        }

        private static void OnOpen(IWebSocketConnection conn)
        {
            Console.WriteLine($@"[{conn.ConnectionInfo.ClientIpAddress}] Open");
        }

        private static void OnClose(IWebSocketConnection conn)
        {
            Console.WriteLine($@"[{conn.ConnectionInfo.ClientIpAddress}] Close");
        }

        private static void OnError(IWebSocketConnection conn, Exception ex)
        {
            Console.WriteLine($@"[{conn.ConnectionInfo.ClientIpAddress}] Err {ex}");
        }

        public static string PackMessage(int msg, params string[] parts)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(msg);

            foreach (string part in parts)
            {
                sb.Append("\t");
                sb.Append(part);
            }

            return sb.ToString();
        }

        public static string PackMessage(SockChatClientMessage msg, params string[] parts)
            => PackMessage((int)msg, parts);

        public static string PackBotMessage(int type, string id, params string[] parts)
        {
            return type.ToString() + '\f' + id + '\f' + string.Join('\f', parts);
        }

        public static void CheckPings()
        {
            List<SockChatUser> users = new List<SockChatUser>(Users);

            foreach (SockChatUser user in users)
            {
                List<SockChatConn> conns = new List<SockChatConn>(user.Connections);

                foreach (SockChatConn conn in conns)
                {
                    if (conn.HasTimedOut)
                    {
                        user.Connections.Remove(conn);
                        conn.Close();
                        Console.WriteLine($@"Nuked a connection from {user.Username} {conn.HasTimedOut} {conn.Websocket.IsAvailable}");
                    }

                    if (user.Connections.Count < 1)
                        UserLeave(null, user, LEAVE_TIMEOUT);
                }
            }
        }

        public static SockChatUser FindUserById(int userId)
        {
            return Users.FirstOrDefault(x => x.UserId == userId);
        }

        public static SockChatUser FindUserBySock(IWebSocketConnection conn)
        {
            return Users.FirstOrDefault(x => x.Connections.Any(y => y.Websocket == conn));
        }

        public static SockChatChannel FindChannelByName(string name)
        {
            return Channels.FirstOrDefault(x => x.Name.ToLowerInvariant().Trim() == name.ToLowerInvariant().Trim());
        }

        public static SockChatMessage[] GetChannelBacklog(SockChatChannel chan, int count = 15)
        {
            return Messages.Where(x => x.Channel == chan || x.Channel == null).Reverse().Take(count).Reverse().ToArray();
        }

        public static void UserJoin(SockChatUser user, FlashiiAuthResult auth, IWebSocketConnection conn)
        {
            SockChatChannel chan = FindChannelByName(auth.DefaultChannel);
            // umi eats the first message for some reason
            conn.Send(PackMessage(SockChatClientMessage.ContextPopulate, @"1", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), user.ToString(), PackBotMessage(0, @"say", @""), @"welcome", @"0", @"1001"));
            conn.Send(PackMessage(SockChatClientMessage.ContextPopulate, @"1", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), user.ToString(), PackBotMessage(0, @"say", $@"Welcome to the temporary drop in chat, {user.Username}!"), @"welcome", @"0", @"1001"));
            HandleJoin(user, chan, conn);
        }

        public const string LEAVE_NORMAL = @"leave";
        public const string LEAVE_KICK = @"kick";
        public const string LEAVE_FLOOD = @"flood";
        public const string LEAVE_TIMEOUT = @"timeout";

        public static void UserLeave(SockChatChannel chan, SockChatUser user, string type = LEAVE_NORMAL)
        {
            if(chan == null)
            {
                Channels.Where(x => x.Users.Contains(user)).ToList().ForEach(x => UserLeave(x, user, type));
                return;
            }

            if(chan.IsTemporary && chan.Owner == user)
            {
                // nuke channel
            }

            chan.UserLeave(user);
            HandleLeave(chan, user, type);
        }

        public static void HandleLeave(SockChatChannel chan, SockChatUser user, string type = LEAVE_NORMAL)
        {
            chan.Send(PackMessage(SockChatClientMessage.UserDisconnect, user.UserId.ToString(), user.Username, type, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), NextMessageId.ToString(), chan.Name));
        }

        public static void HandleJoin(SockChatUser user, SockChatChannel chan, IWebSocketConnection conn)
        {
            if (!chan.HasUser(user))
                chan.Send(PackMessage(SockChatClientMessage.UserConnect, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), user.ToString(), NextMessageId.ToString()));

            conn.Send(PackMessage(SockChatClientMessage.UserConnect, @"y", user.ToString(), chan.Name));

            if(!chan.HasUser(user))
                LogToChannel(chan, user, PackBotMessage(0, @"join", user.Username), @"10010");

            conn.Send(PackMessage(SockChatClientMessage.ContextPopulate, @"0", chan.GetUsersString(new[] { user })));

            SockChatMessage[] msgs = GetChannelBacklog(chan);

            foreach (SockChatMessage msg in msgs)
                conn.Send(PackMessage(SockChatClientMessage.ContextPopulate, @"1", msg.GetLogString()));

            SockChatChannel[] chans = Channels.Where(x => user.Hierarchy >= x.Hierarchy).ToArray();
            StringBuilder sb = new StringBuilder();

            sb.Append(chans.Length);

            foreach (SockChatChannel c in chans)
            {
                sb.Append('\t');
                sb.Append(c);
            }

            conn.Send(PackMessage(SockChatClientMessage.ContextPopulate, @"2", sb.ToString()));

            if (!chan.HasUser(user))
                chan.UserJoin(user);

            if (!Users.Contains(user))
                Users.Add(user);
        }

        public static void LogToChannel(SockChatChannel chan, SockChatUser user, string text, string flags)
        {
            //
        }

        public static FlashiiAuthResult FlashiiAuth(int userId, string token, string ip, string endpoint = "https://flashii.net/_sockchat.php?user_id={0}&token={1}&ip={2}")
        {
            try
            {
                using (WebClient wc = new WebClient())
                    return JsonConvert.DeserializeObject<FlashiiAuthResult>(wc.DownloadString(string.Format(endpoint, userId, token, ip)));
            }
            catch
            {
                return new FlashiiAuthResult { Success = false };
            }
        }

        public static string SanitiseMessage(string input)
        {
            return input.Replace(@"<", @"&lt;").Replace(@">", @"&gt;").Replace("\n", @" <br/> ").Replace("\t", @"    ");
        }

        public static string SanitiseUsername(string input)
        {
            return input.Replace(' ', '_').Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\f", string.Empty).Replace("\t", string.Empty);
        }

        private static void OnMessage(IWebSocketConnection conn, string msg)
        {
            CheckPings();

            Console.WriteLine($@"[{conn.ConnectionInfo.ClientIpAddress}] {msg}");

            string[] args = msg.Split('\t');

            if (args.Length < 1 || !Enum.TryParse(args[0], out SockChatServerMessage opCode))
                return;

            switch (opCode)
            {
                case SockChatServerMessage.Ping:
                    if (!int.TryParse(args[1], out int userId))
                        break;

                    SockChatUser puser = Users.FirstOrDefault(x => x.UserId == userId);

                    if (puser == null)
                        break;

                    SockChatConn pconn = puser.GetConnection(conn);

                    if (pconn == null)
                        break;

                    pconn.BumpPing();
                    conn.Send(PackMessage(SockChatClientMessage.Pong, @"pong"));
                    break;

                case SockChatServerMessage.Authenticate:
                    SockChatUser aUser = FindUserBySock(conn);

                    if (aUser != null || args.Length < 3 || !int.TryParse(args[1], out int aUserId))
                        break;

                    FlashiiAuthResult auth = FlashiiAuth(aUserId, args[2], conn.ConnectionInfo.ClientIpAddress);

                    if (!auth.Success)
                    {
                        conn.Send(PackMessage(SockChatClientMessage.UserConnect, @"n", @"authfail"));
                        break;
                    }

                    // sockfail and userfail checks
                    /*
                    $reason = Context::allowUser(auth.Username, $conn);

                    if (!empty($reason) && $reason !== 'userfail') {
                        fw_log("Auth failed ({$reason}).");
                        conn.Send(PackMessage(SockChatClientMessage.UserConnect, @"n", $reason));
                        break;
                    }
                     */

            // check ban
            /*
            $ban_length = Context::checkBan($GLOBALS['chat']['AUTOID'] ? null : $aparts[0], $conn->remoteAddress, sanitize_name($aparts[1]));

            if ($ban_length !== false) {
                fw_log("Auth failed, banned until {$ban_length}");
                $conn->send(pack_message(1, ['n', 'joinfail', $ban_length]));
                break;
            }
             */

            /*
            if($reason === 'userfail') {
                $oldUser = Context::getUserById($userId);
                $oldUser->runSock(function($sock) {
                    $sock->close();
                });
                Context::leave($oldUser);
                return;
            }
            */

            aUser = FindUserById(auth.UserId) ?? new SockChatUser(auth);
                    aUser.AddConnection(conn);
                    UserJoin(aUser, auth, conn);
                    break;

                case SockChatServerMessage.MessageSend:
                    if (args.Length < 3 || !int.TryParse(args[1], out int mUserId))
                        break;

                    SockChatUser mUser = FindUserById(mUserId);

                    if (mUser == null || !mUser.HasConnection(conn) || string.IsNullOrEmpty(args[2]))
                        break;

                    string message = string.Join('\t', args.Skip(2)).Trim();

                    if (message.Length > 2000)
                        message = message.Substring(0, 2000);

                    if(message[0] == '/')
                    {
                        string[] parts = message.Substring(1).Split(' ');
                        string command = parts[0].Replace(@".", string.Empty).ToLowerInvariant();

                        for (int i = 1; i < parts.Length; i++)
                            parts[i] = SanitiseMessage(parts[i]);

                        switch(command)
                        {
                            default:
                                break;
                        }

                        // find command
                        Console.WriteLine($@"Running command '{command}'");
                        /*
                        if (!Modules::executeRoutine('onCommandReceive', [$user, &$cmd, &$cmdparts])) {
                            return;
                        }

                        if (!Modules::executeCommand($cmd, $user, $cmdparts)) {
                            Message::privateBotMessage(Constants::MSG_ERROR, 'nocmd', [strtolower($cmd)], $user);
                        } 
                        */
                        break;
                    }

                    message = SanitiseMessage(message);

                    // Message::broadcastUserMessage($user, $out);
                    break;
            }
        }
    }
}
