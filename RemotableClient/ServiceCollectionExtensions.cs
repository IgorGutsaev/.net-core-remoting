using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;

namespace RemotableClient
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemotingClient(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<INetChannel, TcpNetChannel>()
                .AddScoped<IMyService, MyServiceWrapper>(); // proxied service
        }
    }
}