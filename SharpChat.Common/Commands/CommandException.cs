using SharpChat.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Commands {
    public class CommandException : Exception {
        private string ErrorCode { get; }
        private object[] Arguments { get; }

        public CommandException(string errorCode, params object[] args) : base(errorCode) {
            ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
            Arguments = args;
        }

        public IServerPacket ToPacket() {
            return new LegacyCommandResponse(ErrorCode, true, Arguments);
        }
    }
}
