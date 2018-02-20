using RemotableInterfaces;
using System;
using System.Net;

namespace RemotableInterfaces
{
    public interface INetChannel
    {
        event EventHandler<string> OnChannelReport;
        event EventHandler<ServiceEvent> OnEvent;

        string Id { get; }

        void Start(INetServerSettings serverSettings);
        void Stop();
        object Invoke(object outgoingMessage, IPEndPoint endpoint = null);
        void Send(NetPackage package, Action<object> handleResult, IPEndPoint destination = null);        
        IPEndPoint GetCallbackAddress();

        void SetHandlerIdentifier(string identifier);
        bool IsEnable();
    }
}