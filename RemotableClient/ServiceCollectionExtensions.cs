using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using RemotableObjects;
using System.Net;

namespace RemotableClient
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemotingClient(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<RemotingClientSetup>()
                .AddSingleton<IRemotingClient>(sp => { return new RemotingClient(new NetServerSettings("127.0.0.1", 65433),
                    sp.GetRequiredService<INetChannel>(),
                    sp.GetRequiredService<RemotingClientSetup>()
                    ); }) // AddTransient
                .AddScoped<IMyService>(sp => { return RemoteDecorator<IMyService>.Create(new MyService(), sp.GetRequiredService<IRemotingClient>()); });
        }
    }
}