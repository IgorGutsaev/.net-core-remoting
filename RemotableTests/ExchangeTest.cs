using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using RemotableServer;
using System;
using System.Diagnostics;
using Xunit;

namespace RemotableTests
{
    public class ExchangeTest : BaseExchangeTest
    {
        public ExchangeTest()
            :base()
        {
                
        }

        [Fact]
        public void Server_Creating()
        {
            // Prepare
            RemotingServer server = this._Provider.GetRequiredService<RemotingServer>();

            // Pre-validate
            Assert.False(server.IsEnable());

            // Perform
            server.Start();

            // Post-validate
            Assert.True(server.IsEnable());

            server.Dispose();
        }

        [Fact]
        public void Server_Client_Start_Stop_NoException()
        {
            int i = 0;
            while (i < 3)
            {
                // Prepare
                RemotingServer server = this._Provider.GetRequiredService<RemotingServer>();
                server.Start();
                IRemotingClient client = this._Provider.GetRequiredService<IRemotingClient>();

                // Pre-validate
                Assert.True(server.IsEnable());

                // Perform
                client.Dispose();
                server.Dispose();

                // Post-validate
                Assert.False(server.IsEnable());
                i++;
             }
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetUnitFromDataGenerator), MemberType = typeof(TestDataGenerator))]
        public void Server_Method_Invocation(int valueInt, string valueString, Unit unit)
        {
            EventHandler<Part> onDetect = (sender, e) => {
                Debug.WriteLine($"{e.GetType().ToString()} detected: {e}");
                Assert.NotNull(e);
            };

            RemotingServer server = this._Provider.GetRequiredService<RemotingServer>();
            server.Start();

            IRemotingClient client = this._Provider.GetRequiredService<IRemotingClient>();

            IMyService service = this._Provider.GetRequiredService<IMyService>();
            service.OnSomeBDetect += onDetect;

            Assert.NotNull(service.Do(valueInt, valueString, unit));


            client.Dispose();
            server.Dispose();
        }

        [Fact]
        public void Test_Exception()
        {
            // Prepare
            RemotingServer server = this._Provider.GetRequiredService<RemotingServer>();
            server.Start();
            IMyService service = this._Provider.GetRequiredService<IMyService>();

            // Pre-validate
            Assert.True(server != null && service != null);

            // Perform
            Exception ex = Assert.Throws<Exception>(() => { service.CheckUnitNotNull(null); });

            // Post-validate
            Assert.Equal("Value cannot be null.\r\nParameter name: Argument must declared!", ex.Message);
            server.Dispose();
        }

        [Fact]
        public void Test_Server_Setup()
        {
            // Prepare
            RemotingServerSetup setup = new RemotingServerSetup();
            setup.PublishService<IMyService>();

            // Pre-validate
            Assert.Contains(setup.PublishedServices, x => x == typeof(IMyService));

            // Perform
            setup.PublishService<IMyService>();

            // Post-validate
            Assert.Single(setup.PublishedServices);
        }
    }
}