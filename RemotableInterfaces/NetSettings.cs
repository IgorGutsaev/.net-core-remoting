using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RemotableInterfactes
{
    public class NetClientSettings
        : INetClientSettings
    {
        public IPAddress ServerIpAddress { get; set; } = IPAddress.Parse("127.0.0.1");
        public Int32 ServerPortNumber { get; set; } = 65432;
    }

    public class NetServerSettings
    : INetServerSettings
    {
        public IPAddress ServerIpAddress { get; set; } = IPAddress.Parse("127.0.0.1");
        public Int32 ServerPortNumber { get; set; } = 65431;
    }
}
