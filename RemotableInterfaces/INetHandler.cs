using Google.Protobuf;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace RemotableInterfaces
{
    public delegate void onMessageRaised(IMessage message, Action<byte[]> onProcess);

    public interface INetHandler
    {
        event onMessageRaised OnMessageRaised;

        void Process(Stream stream, Action<NetPackage> responseAction);

        NetPackage Pack(object data);
    }
}