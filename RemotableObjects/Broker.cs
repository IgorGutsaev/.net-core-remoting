using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableObjects
{
    public class Broker : List<ServerProxy>, IBroker
    {
        private ServiceProvider Provider;

        public Broker(INetListenerHandler handler)
        {
            handler.MessageRaised += Handler_MessageRaised;

            this.Provider = new ServiceCollection()
             .AddScoped<IMyService, MyService>()
             .BuildServiceProvider();
        }

        public string CreateService(string ServiceName)
        {
            string name = ServiceName.ToLowerInvariant().Trim();

            switch (name)
            {
                case "myservice":
                    IMyService myService = this.Provider.GetRequiredService<IMyService>();

                    break;
                default:
                    throw new ArgumentException($"Cannot create Unknown service '{ServiceName}'!");
            }
        }

        private void Handler_MessageRaised(object sender, ExchangeMessage e)
        {
        }
    }
}
