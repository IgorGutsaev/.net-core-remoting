using System;

namespace RemotableInterfaces
{
    public interface IRemotingClient : IDisposable
    {
        event EventHandler<ServiceEvent> OnEvent;

        void CheckBindings();
        object InvokeMethod(string serviceUid, string methodName, MethodParameter[] parameters);
        string BuildRemoteService(Type interfaceType);
    }
}
