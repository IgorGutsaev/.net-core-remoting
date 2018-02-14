using RemoteCommunication.RemotableProtocol;
using System.Net;

namespace RemotableInterfaces
{
    public interface IClientProxy
    {
        T InvokeMethod<T>(string methodName, MethodParameterMsg[] parameters);
        void BuildRemoteService(string interfaceName);
    }
}
