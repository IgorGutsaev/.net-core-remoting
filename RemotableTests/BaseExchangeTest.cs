using Microsoft.Extensions.DependencyInjection;
using RemotableClient;
using RemotableInterfaces;
using RemotableObjects;
using RemotableServer;
using System.Collections.Generic;

namespace RemotableTests
{
    public class BaseExchangeTest
    {
        protected ServiceProvider _Provider;

        public BaseExchangeTest()
        {
            this._Provider = new ServiceCollection()
                .AddRemoting()
                .AddRemotingServer("127.0.0.1", 65432)
                .AddRemotingClient("127.0.0.1", 65433)
                .AddRemotingServices()
                .BuildServiceProvider();
        }
    }
}
