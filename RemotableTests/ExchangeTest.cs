using Microsoft.Extensions.DependencyInjection;
using RemotableClient;
using RemotableInterfaces;
using RemotableObjects;
using RemotableServer;
using System;
using System.Diagnostics;
using Xunit;

namespace RemotableTests
{
    public class ExchangeTest
    {
        [Fact]
        public void Test_Communication()
        {
            ServiceProvider provider = new ServiceCollection()
                .AddRemoting()
                .AddRemotingServer()
                .AddRemotingClient()
                .BuildServiceProvider();


            RemotingServer server = provider.GetRequiredService<RemotingServer>();
            IMyService service = provider.GetRequiredService<IMyService>();
            service.OnSomeBDetect += Service_OnSomeBDetect;

            var result = service.Do(1, "lol", new SomeClassA { Date = DateTime.Now, Uid = "Uid-1", Value = 3,  Child = new SomeClassB { Value = 23, Uid = "Uid-2" } });

            Console.ReadLine();
        }

        private void Service_OnSomeBDetect(object sender, SomeClassB e)
        {
            Debug.WriteLine(e.GetType().ToString() + " detected!");
        }
    }
}
 