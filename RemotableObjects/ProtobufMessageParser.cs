using System;
using RemoteCommunication.RemotableProtocol;
using System.Diagnostics;
using System.IO;
using Google.Protobuf;

namespace RemotableObjects
{
    public class ProtobufMessageParser
    {
        public static IMessage GetMessage(Stream stream)
        {
            byte[] totalLenBuff = new byte[4];
            stream.Read(totalLenBuff, 0, totalLenBuff.Length);
            int totalLength = BitConverter.ToInt32(totalLenBuff, 0);

            if (totalLength == 0) // no return 
                return null;

            var buffer = new byte[totalLength];

            stream.Read(buffer, 0, buffer.Length);

            IMessage incomeMessage = null;

            using (MemoryStream incomeMs = new MemoryStream(buffer))
            {
                byte[] messageTypeHeader = new byte[4]; // to indentify the object
                incomeMs.Read(messageTypeHeader, 0, 4);

                int objectType = BitConverter.ToInt16(messageTypeHeader, 0);

                RemotingCommands messageType = (RemotingCommands)objectType;

                switch (messageType)
                {
                    case RemotingCommands.Unknown:
                        break;
                    case RemotingCommands.ConnectionRequest:
                        incomeMessage = ConnectRequestMsg.Parser.ParseFrom(incomeMs);
                        break;
                    case RemotingCommands.ConnectionResponse:
                        incomeMessage = ConnectResponseMsg.Parser.ParseFrom(incomeMs);
                        break;
                    case RemotingCommands.QueryInterface:
                        incomeMessage = QueryInterfaceMsg.Parser.ParseFrom(incomeMs);
                        break;
                    case RemotingCommands.QueryInterfaceResponse:
                        incomeMessage = QueryInterfaceResponseMsg.Parser.ParseFrom(incomeMs);
                        break;
                    case RemotingCommands.InvokeMethod:
                        incomeMessage = InvokeMethodMsg.Parser.ParseFrom(incomeMs);
                        break;
                    case RemotingCommands.InvokeMethodResponse:
                        incomeMessage = InvokeMethodResponseMsg.Parser.ParseFrom(incomeMs);
                        break;
                    case RemotingCommands.Exception:
                        incomeMessage = RemotingExceptionMsg.Parser.ParseFrom(incomeMs);
                        break;
                    case RemotingCommands.TriggerEvent:
                        incomeMessage = TriggerEventMsg.Parser.ParseFrom(incomeMs);
                        break;
                    default:
                        Debug.WriteLine("Fatal error.");
                        throw new CommunicationException("Unknown messageType");
                }
            }

            if (incomeMessage is RemotingExceptionMsg)
                throw new Exception(((RemotingExceptionMsg)incomeMessage).Message);

            return incomeMessage;
        }
    }
}