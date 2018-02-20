using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using RemotableObjects;

namespace RemotableServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemoting(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddTransient<IBroker, Broker>()
                .AddTransient<INetHandler, NetHandler>()
                .AddTransient<INetChannel, TcpNetChannel>();
        }

        public static IServiceCollection AddRemotingServer(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<RemotingServer>(sp => {
                return new RemotingServer(new NetServerSettings(), sp.GetRequiredService<INetChannel>()); });
        }
    }
}
