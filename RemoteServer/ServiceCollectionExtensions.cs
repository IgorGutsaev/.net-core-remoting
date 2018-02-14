using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using RemotableObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemoting(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<INetServerSettings, NetServerSettings>()
                .AddScoped<INetHandler>(sp => new NetHandler())
                .AddScoped<IClientProxy, ClientProxy>();
        }

        public static IServiceCollection AddRemotingServer(this IServiceCollection serviceCollection)
        {
            return serviceCollection//.AddSingleton<INetListener, NetListener>()
                .AddSingleton<IBroker, Broker>();
        }
    }
}
