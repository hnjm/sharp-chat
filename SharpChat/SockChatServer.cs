using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat
{
    public class SockChatServer : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public static readonly SockChatUser Bot = new SockChatUser
        {
            UserId = -1,
            Username = @"ChatBot",
            Hierarchy = 0,
            Colour = @"inherit",
        };

        public readonly WebSocketServer Server;
        public readonly SockChatContext Context;

        public SockChatServer(ushort port)
        {
            Logger.Write("Starting Sock Chat server...");

            Context = new SockChatContext(this);

            Context.AddChannel(new SockChatChannel {
                Name = @"Lounge",
            });

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
            Logger.Write($@"[{conn.RemoteAddress()}] Open");
            Context.CheckPings();
        }

        private void OnClose(IWebSocketConnection conn)
        {
            Logger.Write($@"[{conn.RemoteAddress()}] Close");

            SockChatUser user = Context.FindUserBySock(conn);

            if (user != null)
                Context.UserLeave(null, user);

            Context.CheckPings();
        }

        private void OnError(IWebSocketConnection conn, Exception ex)
        {
            Logger.Write($@"[{conn.ConnectionInfo.ClientIpAddress}] Err {ex}");
            Context.CheckPings();
        }

        private void OnMessage(IWebSocketConnection conn, string msg)
        {
            Context.CheckPings();

            Logger.Write($@"[{conn.ConnectionInfo.ClientIpAddress}] {msg}");

            string[] args = msg.Split('\t');

            if (args.Length < 1 || !Enum.TryParse(args[0], out SockChatServerMessage opCode))
                return;

            switch (opCode)
            {
                case SockChatServerMessage.Ping:
                    if (!int.TryParse(args[1], out int userId))
                        break;

                    SockChatUser puser = Context.Users.FirstOrDefault(x => x.UserId == userId);

                    if (puser == null)
                        break;

                    SockChatConn pconn = puser.GetConnection(conn);

                    if (pconn == null)
                        break;

                    pconn.BumpPing();
                    conn.Send(SockChatClientMessage.Pong, @"pong");
                    break;

                case SockChatServerMessage.Authenticate:
                    SockChatUser aUser = Context.FindUserBySock(conn);

                    if (aUser != null || args.Length < 3 || !int.TryParse(args[1], out int aUserId))
                        break;

                    FlashiiAuth auth = FlashiiAuth.Attempt(aUserId, args[2], conn.RemoteAddress());

                    if (!auth.Success)
                    {
                        conn.Send(SockChatClientMessage.UserConnect, @"n", @"authfail");
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

                    aUser = Context.FindUserById(auth.UserId);

                    if (aUser == null)
                        aUser = new SockChatUser(auth);
                    else
                    {
                        aUser.ApplyAuth(auth);
                        aUser.Channel?.UpdateUser(aUser);
                    }

                    aUser.AddConnection(conn);

                    SockChatChannel chan = Context.FindChannelByName(auth.DefaultChannel) ?? Context.Channels.FirstOrDefault();

                    // umi eats the first message for some reason so we'll send a blank padding msg
                    conn.Send(SockChatClientMessage.ContextPopulate, Constants.CTX_MSG, Utils.UnixNow, Bot.ToString(), SockChatMessage.PackBotMessage(0, @"say", @""), @"welcome", @"0", @"10010");
                    conn.Send(SockChatClientMessage.ContextPopulate, Constants.CTX_MSG, Utils.UnixNow, Bot.ToString(), SockChatMessage.PackBotMessage(0, @"say", $@"Welcome to the temporary drop in chat, {aUser.Username}!"), @"welcome", @"0", @"10010");

                    Context.HandleJoin(aUser, chan, conn);
                    break;

                case SockChatServerMessage.MessageSend:
                    if (args.Length < 3 || !int.TryParse(args[1], out int mUserId))
                        break;

                    SockChatUser mUser = Context.FindUserById(mUserId);
                    SockChatChannel mChan = Context.FindUserChannel(mUser);

                    if (mUser == null || !mUser.HasConnection(conn) || string.IsNullOrEmpty(args[2]))
                        break;

                    string message = string.Join('\t', args.Skip(2)).Trim();

                    if (message.Length > 2000)
                        message = message.Substring(0, 2000);

                    if (message[0] == '/')
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
                                    : (parts[1].Length > 5 ? parts[1].Substring(0, 5) : parts[1]).ToUpperInvariant().Trim();

                                if(!mUser.IsAway && !string.IsNullOrEmpty(afkStr))
                                {
                                    mUser.IsAway = true;
                                    mUser.Nickname = @"&lt;" + afkStr + @"&gt;_" + mUser.DisplayName;
                                    mChan.UpdateUser(mUser);
                                }
                                break;
                            case @"nick": // sets a temporary nickname
                                if (!mUser.CanChangeNick)
                                {
                                    mUser.Send(true, @"cmdna", @"/nick");
                                    break;
                                }

                                string nickStr = string.Join('_', parts.Skip(1)).Trim().SanitiseUsername();

                                if (nickStr.Length > 15)
                                    nickStr = nickStr.Substring(0, 15);
                                else if (string.IsNullOrEmpty(nickStr))
                                    nickStr = null;

                                if(nickStr != null && Context.FindUserByName(nickStr) != null)
                                {
                                    mUser.Send(true, @"nameinuse", nickStr);
                                    break;
                                }

                                mChan.Send(false, @"nick", mUser.DisplayName, nickStr);
                                mUser.Nickname = nickStr;
                                mChan.UpdateUser(mUser);
                                break;
                            case @"whisper": // sends a pm to another user
                            case @"msg":
                                if (parts.Length < 3)
                                {
                                    mUser.Send(true, @"cmderr");
                                    break;
                                }

                                SockChatUser whisperUser = Context.FindUserByName(parts[1]);

                                if(whisperUser == null)
                                {
                                    mUser.Send(true, @"usernf", parts[1]);
                                    break;
                                }

                                string whisperStr = string.Join(' ', parts.Skip(2));

                                whisperUser.Send(mUser, whisperStr, @"10011");
                                mUser.Send(mUser, $@"{whisperUser.DisplayName} {whisperStr}", @"10011");
                                break;
                            case @"action": // describe an action
                            case @"me":
                                if (parts.Length < 2)
                                    break;

                                string actionMsg = @"<i>" + string.Join(' ', parts.Skip(1)) + @"</i>";
                                if(!string.IsNullOrWhiteSpace(actionMsg))
                                    mChan.Send(mUser, actionMsg, @"11000");
                                break;
                            case @"who": // gets all online users/online users in a channel if arg
                                break;

                            // anyone can use these
                            case @"join": // join a channel
                                break;
                            case @"create": // create a new channel
                                break;
                            case @"delchan": // delete a channel
                                break;
                            case @"password": // set a password on the channel
                            case @"pwd":
                                break;
                            case @"privilege": // sets a minimum hierarchy requirement on the channel
                            case @"rank":
                            case @"priv":
                                break;

                            case @"say": // pretend to be the bot
                                break;
                            case @"delmsg": // deletes a message
                                break;
                            case @"kick": // kick a user from the server
                                break;
                            case @"ban": // ban a user from the server
                                break;
                            case @"pardon": // unban a user
                            case @"unban":
                                break;
                            case @"pardonip": // unban an ip
                            case @"unbanip":
                                break;
                            case @"bans": // gets a list of bans
                            case @"banned":
                                break;
                            case @"silence": // silence a user
                                break;
                            case @"unsilence": // unsilence a user
                                break;
                            case @"ip": // gets a user's ip (from all connections in this case)
                            case @"whois":
                                break;

                            default:
                                mUser.Send(true, @"nocmd", command);
                                break;
                        }
                        break;
                    }

                    mChan.Send(
                        mUser,
                        message.SanitiseMessage()
                    );
                    break;
            }
        }

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
