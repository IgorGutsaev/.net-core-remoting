using RemotableInterfaces;
using System;
using System.Net;

namespace RemotableInterfaces
{
    public interface INetChannel
    {
        event EventHandler<ServiceEvent> OnEvent;

        void Start();
        void Stop();
        bool Connect();
        object Invoke(object outgoingMessage);
        void Send(NetPackage package, Action<object> handleResult, IPEndPoint destination = null);        
        IPEndPoint GetCallbackAddress();
    }
}