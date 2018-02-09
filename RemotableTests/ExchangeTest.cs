using Microsoft.Extensions.DependencyInjection;
using RemotableClient;
using RemotableInterface;
using RemotableInterfaces;
using RemotableInterfactes;
using RemotableServer;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace RemotableTests
{
    public class ExchangeTest
    {
        [Fact]
        public void Test_Communication()
        {
            INetChannelActivator activator = new ServiceCollection()
                .AddRemoting()
                .AddSingleton<INetChannelActivator>(sp=> {
                    return new NetChannelActivator(sp.GetRequiredService<INetServerSettings>(), sp.GetRequiredService<INetListenerHandler>().Handle);
                })
                .BuildServiceProvider()
                .GetRequiredService<INetChannelActivator>();

            activator.Start();


            IMyService service = new ServiceCollection()
            .AddRemoting()
            .AddScoped<IMyService, MyServiceWrapper>()
            .BuildServiceProvider()
            .GetRequiredService<IMyService>();

         //   service.Do();

            System.Threading.Thread.Sleep(100000);

            Console.ReadLine();
        }
    }
}
