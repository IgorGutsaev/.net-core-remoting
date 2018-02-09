using Google.Protobuf;
using RemotableInterfaces;
using RemotableInterfactes;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RemotableServer
{
    public interface INetChannel
    {
        bool Start();
        void Send(NetPackage package);
        void Disconnect();
    }
}