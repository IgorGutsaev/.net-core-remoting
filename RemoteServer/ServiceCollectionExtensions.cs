using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using RemotableObjects;

namespace RemotableServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemoting(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<INetServerSettings, NetServerSettings>()
                .AddTransient<INetChannel, TcpNetChannel>()
                .AddSingleton<INetHandler, NetHandler>()
                .AddScoped<IBroker, Broker>()
                .AddTransient<IClientProxy, ClientProxy>();
        }

        public static IServiceCollection AddRemotingServer(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<RemotingServer>();
        }
    }
}
