using System;
using System.Collections.Generic;

namespace RemotableInterfaces
{
    public interface IServerProxy: IDisposable
    {
        string Uid { get; }
        object InvokeMethod(string methodName, List<MethodParameter> parameters);
    }
}
