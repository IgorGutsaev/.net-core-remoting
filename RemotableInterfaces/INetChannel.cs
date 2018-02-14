using RemotableInterfaces;
using System;
using System.Net;

namespace RemotableInterfaces
{
    public interface INetChannel
    {
        void Start();
        void Send(NetPackage package, Action<byte[]> handler);
        void Disconnect();
    }
}