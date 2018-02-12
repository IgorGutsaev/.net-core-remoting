using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RemotableInterfactes
{
    public class NetServerEndpointSettings
        : INetServerEndpointSettings
    {
        public IPAddress ServerIpAddress { get; set; } = IPAddress.Parse("127.0.0.1");
        public Int32 ServerPortNumber { get; set; } = 65431;
    }
}
