using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableInterfaces
{
    public interface IBroker
    {
        string CreateProxy(string serviceName);
    }
}
