using SharpChat.Events;
using SharpChat.Messages;
using SharpChat.Packets;
using SharpChat.Sessions;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Channels {
    public class ChannelUserRelations : IEventHandler {
        private IEventDispatcher Dispatcher { get; }
        private ChannelManager Channels { get; }
        private UserManager Users { get; }
        private SessionManager Sessions { get; }
        private MessageManager Messages { get; }
        private readonly object Sync = new object();

        public ChannelUserRelations(
            IEventDispatcher dispatcher,
            ChannelManager channels,
            UserManager users,
            SessionManager sessions,
            MessageManager messages
        ) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            Users = users ?? throw new ArgumentNullException(nameof(users));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        }

        public bool HasUser(IChannel channel, IUser user) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            if(Channels.GetChannel(channel) is not Channel c)
                return false;

            user = Users.GetUser(user);
            if(user == null)
                return false;

            return c.HasUser(user);
        }
        
        public void GetUsers(IChannel channel, Action<IEnumerable<IUser>> callback) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            if(Channels.GetChannel(channel) is Channel c)
                c.GetUsers(callback);
        }

        public void GetChannels(IUser user, Action<IEnumerable<IChannel>> callback) {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            if(callback == null)
                throw new ArgumentNullException(nameof(callback));

            if(Users.GetUser(user) is User u)
                u.GetChannels(cn => Channels.GetChannels(cn, callback));
        }

        public void JoinChannel(IChannel channel, IUser user) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            if(!HasUser(channel, user))
                Dispatcher.DispatchEvent(this, new ChannelJoinEvent(channel, user));
        }

        public void LeaveChannel(IChannel channel, IUser user, UserDisconnectReason reason = UserDisconnectReason.Unknown) {
            if(channel == null)
                throw new ArgumentNullException(nameof(channel));
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            if(HasUser(channel, user))
                Dispatcher.DispatchEvent(this, new ChannelLeaveEvent(channel, user, reason));
        }

        public void HandleEvent(object sender, IEvent evt) {
            lock(Sync) {
                switch(evt) {
                    case ChannelJoinEvent cje:
                        // THIS DOES NOT DO WHAT YOU WANT IT TO DO
                        // I THINK
                        // it really doesn't, figure out how to leave channels when MCHAN isn't active for the session
                        //if((Sessions.GetCapabilities(cje.User) & ClientCapability.MCHAN) == 0)
                        //    LeaveChannel(cje.Channel, cje.User, UserDisconnectReason.Leave);
                        break;

                    case ChannelLeaveEvent cle: // Should ownership just be passed on to another user instead of Destruction?
                        IChannel channel = Channels.GetChannel(evt.Channel);
                        if(channel.IsTemporary && evt.User.Equals(channel.Owner))
                            Channels.Remove(channel);
                        break;

                    case MessageUpdateEvent mue: // there should be a v2cap that makes one packet, this is jank
                        IMessage msg = Messages.GetMessage(mue.MessageId);
                        evt = msg == null
                            ? new MessageDeleteEvent(mue)
                            : new MessageUpdateEventWithData(mue, msg);
                        break;
                }

                if(evt.Channel != null)
                    GetUsers(evt.Channel, users => Sessions.GetLocalSessions(users, sessions => {
                        foreach(ILocalSession session in sessions)
                            session.HandleEvent(sender, evt);
                    }));
            }
        }
    }
}
