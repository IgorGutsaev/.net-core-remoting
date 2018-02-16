using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RemotableClient
{
    internal abstract class BaseServiceProxy : IDisposable
    {
        private IClientProxy _proxy;

        public BaseServiceProxy(IClientProxy proxy)
        {
            this._proxy = proxy;

            string interfaceName = this.GetType().GetInterfaces()[1].Name;

            this._proxy.BuildRemoteService(interfaceName);
            this._proxy.OnEvent += _proxy_OnEvent;
        }

        private void _proxy_OnEvent(object sender, ServiceEvent e)
        {
            var eventInfoes = this.GetType().GetEvents();

            foreach (var eventInfo in eventInfoes)
            {
                Type typeEventParam = eventInfo.EventHandlerType;

                if (typeEventParam.GenericTypeArguments.FirstOrDefault() == e.Data.GetType())
                {
                    var eventDelegate = (MulticastDelegate)this.GetType().GetField(eventInfo.Name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
                    if (eventDelegate != null)
                    {
                        foreach (var handler in eventDelegate.GetInvocationList())
                        {
                            handler.Method.Invoke(handler.Target, new object[] { null, e.Data });
                        }
                    }
                }
            }
        }

        public T InvokeMethod<T>(string methodName, MethodParameter[] parameters)
        {
            return (T)this._proxy.InvokeMethod<T>(methodName, parameters);
        }

        protected MethodParameter[] PrepareParameters(MethodBase mBase, dynamic data)
        {
            List<MethodParameter> result = new List<MethodParameter>();

            int index = 0;
            foreach (var p in mBase.GetParameters())
            {
                var t = data[index];
                index++;

                MethodParameter param = new MethodParameter(p.Name, p.ParameterType, t);
                result.Add(param);
            }

            return result.ToArray();
        }

        public void Dispose()
        {
            this._proxy.Dispose();
        }
    }
}