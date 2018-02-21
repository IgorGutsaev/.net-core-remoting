using System;
using System.IO;
using System.Net;

namespace RemotableInterfaces
{
    public delegate void onEventRaised(ServiceEvent ev, IPEndPoint endpoint);

    public interface INetHandler
    {
        event onEventRaised OnEventRaised;

        NetPackage ProcessRequest(Stream stream, Action<object> handleResult);

        NetPackage Pack(object data);
    }
}