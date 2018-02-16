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

        private ServiceProvider Provider;

        private IServerProxy this[string serviceUid]
        {
            get
            {
                return this.FirstOrDefault(x => String.Equals(x.Uid, serviceUid.Trim(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public Broker()
        {
            this.Provider = new ServiceCollection()
             .AddScoped<IMyService, MyService>()
             .BuildServiceProvider();
        }

        public string CreateService(string serviceName, IPEndPoint endpoint)
        {
            string name = serviceName.ToLowerInvariant().Trim();
            string serviceGuid = "";

            switch (name)
            {
                case "imyservice":
                    IMyService myService = this.Provider.GetRequiredService<IMyService>();
                    ServerProxy<IMyService> proxy = new ServerProxy<IMyService>(myService, endpoint);
                    if (proxy.isCreated)
                    {
                        this.Add(proxy);
                        proxy.OnEventRaised += (serviceUid, someEvent, callbackEndpoint) => { this.OnEventRaised?.Invoke(new ServiceEvent { ServiceUid = serviceUid, Data = someEvent }, callbackEndpoint); };
                    }

                    serviceGuid = proxy.Uid;
                    break;
                default:
                    throw new ArgumentException($"Cannot create Unknown service '{serviceName}'!");
            }

            return serviceGuid;
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