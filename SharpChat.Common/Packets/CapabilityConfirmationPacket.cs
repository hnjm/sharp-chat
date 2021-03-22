using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class CapabilityConfirmationPacket : IServerPacket {
        private IEnumerable<string> Capabilities { get; }

        private static readonly string[] Names = Enum.GetNames(typeof(ClientCapabilities));
        private static readonly int[] Values = Enum.GetValues(typeof(ClientCapabilities)) as int[];

        public CapabilityConfirmationPacket(ClientCapabilities caps) {
            Capabilities = GetStrings((int)caps);
        }

        private static IEnumerable<string> GetStrings(int caps) {
            for(int i = 0; i < Values.Length; ++i)
                if((caps & Values[i]) > 0)
                    yield return Names[i];
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.CapabilityConfirm);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(string.Join(' ', Capabilities));

            return sb.ToString();
        }
    }
}
