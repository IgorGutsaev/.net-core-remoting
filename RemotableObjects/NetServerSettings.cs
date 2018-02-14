using RemotableInterfaces;
using System.Net;

namespace RemotableObjects
{
    public class NetServerSettings : INetServerSettings
    {
        public IPEndPoint ServerEndpoint { get; private set; }
            = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 65432);

        public IPEndPoint GetServerAddress()
        {
            return ServerEndpoint;
        }

        public override string ToString()
        {
            return ServerEndpoint.ToString();
        }
    }
}
