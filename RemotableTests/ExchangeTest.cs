using Microsoft.Extensions.DependencyInjection;
using RemotableClient;
using RemotableInterfaces;
using RemotableInterfactes;
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
            INetChannelListener activator = new ServiceCollection()
                .AddRemoting()
                .AddSingleton<INetServerEndpointSettings, NetServerEndpointSettings>()
                .AddSingleton<INetChannelListener>(sp => {
                    return new NetChannelListener(sp.GetRequiredService<INetServerEndpointSettings>(), sp.GetRequiredService<INetListenerHandle>().Handle);
                })
                .BuildServiceProvider()
                .GetRequiredService<INetChannelListener>();

            activator.Start();

            IMyService service = new ServiceCollection()
            .AddRemoting()
            .AddSingleton<INetServerEndpointSettings>(sp => { return new NetServerEndpointSettings(); })
            .AddScoped<INetChannel>(sp => { return new NetChannel(sp.GetRequiredService<INetServerEndpointSettings>(), sp.GetRequiredService<INetListenerHandle>().Handle2); })
            .AddScoped<IMyService, MyServiceWrapper>()
            .BuildServiceProvider()
            .GetRequiredService<IMyService>();
            
             //  service.Do();

            System.Threading.Thread.Sleep(100000);

            Console.ReadLine();
        }
    }
}
