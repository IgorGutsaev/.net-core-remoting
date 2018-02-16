using RemoteCommunication.RemotableProtocol;
using System;
using System.Net;

namespace RemotableInterfaces
{
    public interface IClientProxy : IDisposable
    {
        event EventHandler<ServiceEvent> OnEvent;

        T InvokeMethod<T>(string methodName, MethodParameter[] parameters);
        void BuildRemoteService(string interfaceName);
    }
}
