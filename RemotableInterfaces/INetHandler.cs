using Google.Protobuf;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace RemotableInterfaces
{
    public delegate void onEventRaised(ServiceEvent ev, IPEndPoint endpoint);

    public interface INetHandler
    {
        event onEventRaised OnEventRaised;

        NetPackage ProcessRequest(Stream stream, Action<object> handleResult);

        NetPackage Pack(object data);

        void SetHandlerIdentifier(string identifier);
    }
}