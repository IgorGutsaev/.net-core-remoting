using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RemotableObjects
{
    public class Broker : List<IServerProxy>, IBroker
    {
        public event onEventRaised OnEventRaised;

        private ServiceProvider _Provider;

        private IServerProxy this[string serviceUid]
        {
            get
            {
                return this.FirstOrDefault(x => String.Equals(x.Uid, serviceUid.Trim(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public Broker()
        {
            // Create container with local services
            this._Provider = new ServiceCollection()
                .AddServices()
                .BuildServiceProvider();
        }

        public string CreateService(string serviceName, IPEndPoint callbackEndpoint)
        {
            Type interfaceType = Type.GetType(serviceName);

            if (interfaceType == null)
                throw new NotSupportedException($"Cannot resolve service '{serviceName}'");

            var instance = this._Provider.GetRequiredService(interfaceType);

            ServerProxy proxy = new ServerProxy(instance, callbackEndpoint);
            this.Add(proxy);
            proxy.OnEventRaised += Proxy_OnEventRaised;

            return proxy.Uid;
        }

        private void Proxy_OnEventRaised(string serviceUid, object someEvent, IPEndPoint callbackEndpoint)
        {
           this.OnEventRaised?.Invoke(new ServiceEvent { ServiceUid = serviceUid, Data = someEvent }, callbackEndpoint); 
        }

        public object InvokeMethod(string serviceUid, string methodName, List<MethodParameter> parameters)
        {
            var service = this[serviceUid];
            if (service == null)
                throw new CommunicationException($"Service {serviceUid} not found!");

            return service.InvokeMethod(methodName, parameters);
        }

        public void ReleaseService(string serviceName)
        {
            var service = this[serviceName];
            if (service != null)
            {
                this.Remove(service);
                service.Dispose();
            }
        }
    }
}