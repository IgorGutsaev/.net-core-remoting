using Google.Protobuf;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace RemotableInterfaces
{
    public delegate void onMessageRaised(IMessage message, Action<byte[]> onProcess);

    public interface INetListenerHandler
    {
        event onMessageRaised OnMessageRaised;

        void Process(Stream stream, Action<byte[]> responseAction);

        //void Handle2(byte[] data, Action<byte[]> responseAction);
    }
}