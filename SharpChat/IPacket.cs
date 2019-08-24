using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat
{
    public interface IServerPacket
    {
        string Pack(int version, int eventId);
    }
}
