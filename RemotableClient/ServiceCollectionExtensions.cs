using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using RemotableObjects;

namespace RemotableClient
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemotingClient(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddTransient<IMyService>(sp => { return RemoteDecorator<IMyService>.Create(new MyService(), sp.GetRequiredService<IClientProxy>()); });
        }
    }
}