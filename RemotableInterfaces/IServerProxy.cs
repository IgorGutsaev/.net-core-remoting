
using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableInterfaces
{
    public interface IServerProxy: IDisposable
    {
        string Uid { get; }
        object InvokeMethod(string methodName, List<MethodParameter> parameters);
    }
}
