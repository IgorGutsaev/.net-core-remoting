using RemoteCommunication.RemotableProtocol;
using System;
using System.Net;

namespace RemotableInterfaces
{
    public interface IClientProxy : IDisposable
    {
        event EventHandler<ServiceEvent> OnEvent;

        object InvokeMethod(string methodName, MethodParameter[] parameters);
        void BuildRemoteService(string interfaceName);
    }
}
