using Microsoft.Extensions.DependencyInjection;
using RemotableClient;
using RemotableInterfaces;
using RemotableInterfactes;
using RemotableObjects;
using RemotableServer;
using System;
using Xunit;

namespace RemotableTests
{
    public class ExchangeTest
    {
        [Fact]
        public void Test_Communication()
        {
            IBroker broker = new ServiceCollection()
                .AddRemoting()
                .AddSingleton<INetServerEndpointSettings, NetServerEndpointSettings>()
                .AddSingleton<INetListener>(sp => {
                    return new NetListener(sp.GetRequiredService<INetServerEndpointSettings>(), sp.GetRequiredService<INetListenerHandler>().Handle);
                })
                .AddSingleton<IBroker, Broker>()
                .BuildServiceProvider()
                .GetRequiredService<IBroker>();

            IMyService service = new ServiceCollection()
            .AddRemoting()
            .AddSingleton<INetServerEndpointSettings>(sp => { return new NetServerEndpointSettings(); })
            .AddScoped<INetChannel>(sp => { return new NetChannel(sp.GetRequiredService<INetServerEndpointSettings>(), sp.GetRequiredService<INetListenerHandler>().Handle2); })
            .AddScoped<IMyService, MyServiceWrapper>()
            .BuildServiceProvider()
            .GetRequiredService<IMyService>();
            
             service.Do();

            System.Threading.Thread.Sleep(100000);

            Console.ReadLine();
        }
    }
}
