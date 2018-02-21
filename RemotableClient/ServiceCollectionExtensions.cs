using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using RemotableObjects;

namespace RemotableClient
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemotingClient(this IServiceCollection serviceCollection, string address, int port)
        {
            return serviceCollection
                .AddSingleton<IServiceBindings>(sp => { return new ServiceBindings().AddBinding<IMyService>("127.0.0.1:65432"); })
                .AddSingleton<RemotingClientSetup>(sp => { return new RemotingClientSetup(sp.GetRequiredService<IServiceBindings>()); })
                .AddSingleton<IRemotingClient>(sp =>
                {
                    return new RemotingClient(new NetServerSettings(address, port),
                        sp.GetRequiredService<INetChannel>(),
                        sp.GetRequiredService<RemotingClientSetup>());
                });   
        }
    }
}