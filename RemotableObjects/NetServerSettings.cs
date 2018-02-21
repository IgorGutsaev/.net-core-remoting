using RemotableInterfaces;
using System;
using System.Net;

namespace RemotableObjects
{
    public class NetServerSettings : INetServerSettings
    {
        public IPEndPoint ServerEndpoint { get; private set; }

        public NetServerSettings(string ipAddress, int port)
        {
            this.ServerEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }

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
