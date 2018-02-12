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
            byte[] typeBuff = null;
            byte[] bodyBuff = null;

            if (data is IMessage)
                bodyBuff = ((IMessage)data).ToByteArray();

            if (data is ConnectRequestMsg)
                typeBuff = BitConverter.GetBytes((int)RemotingCommands.ConnectionRequest);
            else if (data is ConnectResponseMsg)
                typeBuff = BitConverter.GetBytes((int)RemotingCommands.ConnectionResponse);
            else if (data is QueryInterfaceMsg)
                typeBuff = BitConverter.GetBytes((int)RemotingCommands.QueryInterface);
            else return null;

            byte[] totalLengthBuff = BitConverter.GetBytes(typeBuff.Length + bodyBuff.Length);

            return NetPackage.Create(serviceUid, totalLengthBuff.Combine(typeBuff).Combine(bodyBuff));
        }
    }
}
