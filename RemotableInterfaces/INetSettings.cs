using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RemotableInterfactes
{
    public interface INetServerEndpointSettings
    {
        IPAddress ServerIpAddress { get; set; }
        Int32 ServerPortNumber { get; set; }
    }
}
