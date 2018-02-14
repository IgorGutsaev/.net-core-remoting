using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableInterfaces
{
    public interface INetSenderHandler
    {
        NetPackage Pack(object data);
    }
}
