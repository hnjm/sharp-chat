using SharpChat.Channels;
using SharpChat.Configuration;
using SharpChat.Events;
using SharpChat.Events.Storage;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// THE PLAN:
//  No longer store the entire chain of events in the database, only keep messages.
//  There's a lot of overhead that is ultimate meaningless in the long run.
//  IN other words all the database shit needs to be pulled over here 
//   and events only exist for broadcasting.

namespace SharpChat.Messages {
    public class MessageManager : IEventHandler {
        private IEventDispatcher Dispatcher { get; }
        private IEventTarget Target { get; }
        private IEventStorage Storage { get; }
        private IConfig Config { get; }

        public const int DEFAULT_LENGTH_MAX = 2100;
        private CachedValue<int> TextMaxLengthValue { get; }
        public int TextMaxLength => TextMaxLengthValue;

        private readonly object Sync = new object();

        public MessageManager(IEventDispatcher dispatcher, IEventTarget target, IEventStorage storage, IConfig config) {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            Config = config ?? throw new ArgumentNullException(nameof(config));

            TextMaxLengthValue = config.ReadCached(@"maxLength", DEFAULT_LENGTH_MAX);
        }

        public Message Create(IChannel channel, IUser sender, string text, bool isAction = false) {
            lock(Sync) {
                Message message = new Message(channel, sender, text, isAction);

                Dispatcher.DispatchEvent(this, new MessageCreateEvent(message));

                return message;
            }
        }

        public void Edit(Message message, string text = null) {
            lock(Sync) {
                // retrieve message and update it
            }
        }

        public void Delete(IUser user, Message message) {
            lock(Sync) {
                Dispatcher.DispatchEvent(this, new MessageDeleteEvent(message.Channel, user, message.MessageId));
            }
        }

        public void HandleEvent(object sender, IEvent evt) {
            //
        }
    }
}
