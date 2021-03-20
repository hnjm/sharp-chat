using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Collections.Generic;

namespace SharpChat.Channels {
    public class ChannelUserRelations : IEventHandler {
        private IEventDispatcher Dispatcher { get; }
        private ChannelManager Channels { get; }
        private UserManager Users { get; }

        public ChannelUserRelations(IEventDispatcher dispatcher, ChannelManager channels, UserManager users) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            Users = users ?? throw new ArgumentNullException(nameof(users));
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
            switch(evt) {
                case ChannelLeaveEvent _: // Should ownership just be passed on to another user instead of Destruction?
                    IChannel channel = Channels.GetChannel(evt.Target);
                    if(channel.IsTemporary && evt.Sender.Equals(channel.Owner))
                        Channels.Remove(channel);
                    break;
            }
        }
    }
}
