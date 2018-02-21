using Microsoft.Extensions.DependencyInjection;
using RemotableObjects;
using RemotableServer;
using System;

namespace ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceProvider _Provider = new ServiceCollection()
             .AddRemoting()
             .AddRemotingServer("127.0.0.1", 65432)
             .AddRemotingServices()
             .BuildServiceProvider();

            using (RemotingServer server = _Provider.GetRequiredService<RemotingServer>())
            {
                server.Start();

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}
