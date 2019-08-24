using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat
{
    public interface IPacket
    {
        string Pack(int version);
    }
}
