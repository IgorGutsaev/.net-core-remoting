using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using RemotableInterfaces;
using RemotableObjects;
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

                byte[] typeBuff = BitConverter.GetBytes((int)message.Type);
                byte[] bodyBuff = message.ToByteArray();

                byte[] totalLengthBuff = BitConverter.GetBytes(typeBuff.Length + bodyBuff.Length);

                return NetPackage.Create(serviceUid, totalLengthBuff.Combine(typeBuff).Combine(bodyBuff));
            }

            return null;
        }


    }
}
