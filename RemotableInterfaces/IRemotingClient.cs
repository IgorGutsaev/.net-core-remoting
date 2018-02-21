using System;

namespace RemotableInterfaces
{
    public interface IRemotingClient : IDisposable
    {
        event EventHandler<ServiceEvent> OnEvent;

        object InvokeMethod(string serviceUid, string methodName, MethodParameter[] parameters);
        string InvokeService(Type interfaceType);
    }
}
