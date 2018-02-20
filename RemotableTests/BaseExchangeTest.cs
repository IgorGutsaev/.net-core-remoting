using Microsoft.Extensions.DependencyInjection;
using RemotableClient;
using RemotableInterfaces;
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
                .AddRemotingServer()
                .AddRemotingClient()
                .AddRemotingServices()
                .BuildServiceProvider();
        }
    }
}
