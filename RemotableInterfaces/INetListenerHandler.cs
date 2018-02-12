using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace RemotableInterfaces
{
    public delegate void onMessageRaised(ExchangeMessage message, Action<byte[]> onProcess);

    public interface INetListenerHandler
    {
        event onMessageRaised OnMessageRaised;

        void Handle(Stream stream, EndPoint clientEndPoint, Action<byte[]> responceAction);

        void Handle2(byte[] data, Action<byte[]> responseAction);
    }
}