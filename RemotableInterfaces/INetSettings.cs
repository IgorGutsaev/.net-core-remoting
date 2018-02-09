using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RemotableInterfactes
{
    public interface INetServerSettings
    {
        IPAddress ServerIpAddress { get; set; }
        Int32 ServerPortNumber { get; set; }
    }

    public interface INetClientSettings
    {
        IPAddress ServerIpAddress { get; set; }
        Int32 ServerPortNumber { get; set; }
    }
}
