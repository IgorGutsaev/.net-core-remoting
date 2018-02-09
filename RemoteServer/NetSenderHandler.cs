using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;

namespace RemotableServer
{
    public class NetSenderHandler : INetSenderHandler
    {
        public NetPackage Pack(string serviceUid, object data)
        {
            if (data is ConnectRequestMsg)
            {
                ConnectRequestMsg message = (ConnectRequestMsg)data;
                return NetPackage.Create(serviceUid, message.ToByteArray());
            }

            return null;
        }
    }
}
