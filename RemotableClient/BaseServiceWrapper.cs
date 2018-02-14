using RemotableInterfaces;
using RemotableObjects;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Diagnostics;
using System.Reflection;

namespace RemotableClient
{
    internal abstract class BaseServiceWrapper
    {
        private IClientProxy _proxy;

        public BaseServiceWrapper(IClientProxy proxy)
        {
            this._proxy = proxy;

            string interfaceName = this.GetType().GetInterfaces()[0].Name;

            this._proxy.BuildRemoteService(interfaceName);
        }

        public T InvokeMethod<T>(string methodName, MethodParameterMsg[] parameters)
        {
            return (T)this._proxy.InvokeMethod<T>(methodName, parameters);
        }
    }
}