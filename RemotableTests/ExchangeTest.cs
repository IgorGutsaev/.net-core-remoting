using Microsoft.Extensions.DependencyInjection;
using RemotableClient;
using RemotableInterfaces;
using RemotableObjects;
using RemotableServer;
using System;
using System.Diagnostics;
using System.Linq;
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
        public void Server_Constructor_Creted()
        {
            // Prepare
            RemotingServer server = this._Provider.GetRequiredService<RemotingServer>();

            // Pre-validate


            // Perform
            server.Start();

            // Post-validate

        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetUnitFromDataGenerator), MemberType = typeof(TestDataGenerator))]
        public void Test_Intercept_Event(Unit a1, Unit a2, Unit a3)
        {
            EventHandler<Part> onDetect = (sender, e) => {
                Debug.WriteLine($"{e.GetType().ToString()} detected: {e}");
                Assert.NotNull(e);
            };

            RemotingServer server = this._Provider.GetRequiredService<RemotingServer>();
            server.Start();

            IMyService service = this._Provider.GetRequiredService<IMyService>();
            service.OnSomeBDetect += onDetect;

            Assert.NotNull(service.Do(1, "hi there", a1));
            Assert.NotNull(service.Do(2, "hi there", a2));
            Assert.NotNull(service.Do(3, "hi there", a3));

            server.Stop();
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
        }

        [Fact]
        public void Test_Server_Setup()
        {
            RemotingServerSetup setup = new RemotingServerSetup();
            setup.PublishService<IMyService>();

            Assert.Contains(setup.PublishedServices, x => x == typeof(IMyService));
        }
    }
}
 