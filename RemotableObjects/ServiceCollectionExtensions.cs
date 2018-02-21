using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using RemotableObjects;

namespace RemotableObjects
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemotingServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddScoped<IMyService>(sp => { return RemoteDecorator<IMyService>.Create(new MyService(), sp.GetRequiredService<IRemotingClient>()); });
        }

        public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddScoped<IMyService, MyService>();
        }
    }
}