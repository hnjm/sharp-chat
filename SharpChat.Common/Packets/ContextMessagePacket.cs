using SharpChat.Events;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class ContextMessagePacket : ServerPacketBase {
        public IEvent Event { get; private set; }
        public bool Notify { get; private set; }

        public ContextMessagePacket(IEvent evt, bool notify = false) {
            Event = evt ?? throw new ArgumentNullException(nameof(evt));
            Notify = notify;
        }

        private const string V1_CHATBOT = "-1\tChatBot\tinherit\t";

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ContextPopulate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerContextPacket.Message);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Event.DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);

            switch (Event) {
                case IMessageEvent msg:
                    sb.Append(Event.Sender.Pack());
                    sb.Append(IServerPacket.SEPARATOR);
                    sb.Append(
                        msg.Text
                            .Replace(@"<", @"&lt;")
                            .Replace(@">", @"&gt;")
                            .Replace("\n", @" <br/> ")
                            .Replace("\t", @"    ")
                    );
                    break;

                case UserConnectEvent _:
                    sb.Append(V1_CHATBOT);
                    sb.Append(IServerPacket.SEPARATOR);
                    sb.Append(BotArguments.Notice(@"join", Event.Sender.UserName));
                    break;

                case UserChannelJoinEvent _:
                    sb.Append(V1_CHATBOT);
                    sb.Append(IServerPacket.SEPARATOR);
                    sb.Append(BotArguments.Notice(@"jchan", Event.Sender.UserName));
                    break;

                case UserChannelLeaveEvent _:
                    sb.Append(V1_CHATBOT);
                    sb.Append(IServerPacket.SEPARATOR);
                    sb.Append(BotArguments.Notice(@"lchan", Event.Sender.UserName));
                    break;

                case UserDisconnectEvent ude:
                    string udeReason = ude.Reason switch {
                        UserDisconnectReason.Flood => @"flood",
                        UserDisconnectReason.Kicked => @"kick",
                        UserDisconnectReason.TimeOut => @"timeout",
                        _ => @"leave",
                    };

                    sb.Append(V1_CHATBOT);
                    sb.Append(IServerPacket.SEPARATOR);
                    sb.Append(BotArguments.Notice(udeReason, Event.Sender.UserName));
                    break;
            }


            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Event.EventId < 1 ? SequenceId : Event.EventId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Notify ? '1' : '0');
            sb.Append(IServerPacket.SEPARATOR);
            sb.AppendFormat(
                "1{0}0{1}{2}",
                Event.Flags.HasFlag(EventFlags.Action) ? '1' : '0',
                Event.Flags.HasFlag(EventFlags.Action) ? '0' : '1',
                Event.Flags.HasFlag(EventFlags.Private) ? '1' : '0'
            );

            return sb.ToString();
        }
    }
}
