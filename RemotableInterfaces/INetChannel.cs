using RemotableInterfaces;
using System;
using System.Net;

namespace RemotableInterfaces
{
    public interface INetChannel
    {
        void Start(bool isServer);
        void Send(NetPackage package, Action<byte[]> handler, IPEndPoint destination = null);
        void Disconnect();
    }
}