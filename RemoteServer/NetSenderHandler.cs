using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Google.Protobuf;
using ProtoBuf;
using RemotableInterfaces;
using RemotableObjects;
using RemoteCommunication.RemotableProtocol;

namespace RemotableServer
{
    public class NetSenderHandler : INetSenderHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns>object type in order to not depend on serializer interfaces</returns>
        public NetPackage Pack(object data)
        {
            if (!(data is IMessage))
                throw new CommunicationException("Only IMessage data allowed!");
 
            RemotingCommands type = (RemotingCommands)data.GetType().GetProperty("Type").GetValue(data, null);
            // buffer stores type of message
            byte[] typeBuff = BitConverter.GetBytes((int)type);
            // message body
            byte[] bodyBuff = ((IMessage)data).ToByteArray();
            // buffer stores total length of significant data
            byte[] totalLengthBuff = BitConverter.GetBytes(typeBuff.Length + bodyBuff.Length);
            
            return NetPackage.Create(totalLengthBuff.Combine(typeBuff).Combine(bodyBuff));
        }
    }
}