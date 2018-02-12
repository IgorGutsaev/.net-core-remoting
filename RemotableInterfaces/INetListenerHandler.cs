using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace RemotableInterfaces
{
    public interface INetListenerHandler
    {
        event EventHandler<ExchangeMessage> MessageRaised;

        void Handle(Stream stream, EndPoint clientEndPoint);

        void Handle2(byte[] data);
    }
}