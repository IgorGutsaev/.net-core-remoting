using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RemotableInterfaces
{
    public interface INetServerSettings
    {
        IPEndPoint GetServerAddress();
    }
}
