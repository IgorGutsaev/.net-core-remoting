using System.Collections.Generic;
using System.Net;

namespace RemotableInterfaces
{
    public interface IBroker
    {
        event onEventRaised OnEventRaised;

        string CreateService(string serviceName, IPEndPoint callbackEndpoint);
        void ReleaseService(string serviceName);
        object InvokeMethod(string serviceUid, string methodName, List<MethodParameter> parameters);
    }
}