using System.Collections.Generic;

namespace SharpChat
{
    public interface IServerPacket
    {
        IEnumerable<string> Pack(int version, int eventId);
    }
}
