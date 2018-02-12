using System.Net.Sockets;
using System.Net;
using System;
using RemoteCommunication.RemotableProtocol;
using System.Diagnostics;
using System.IO;
using Google.Protobuf;
using RemotableInterfaces;

namespace RemotableServer
{
    public class NetListenerHandler : INetListenerHandler
    {
        public event EventHandler<ExchangeMessage> MessageRaised;

        public NetListenerHandler()
        {
        }

        public void Handle(Stream stream, EndPoint clientendPoint)
        {
            try
            {
                byte[] totalLenBuff = new byte[4];
                stream.Read(totalLenBuff, 0, totalLenBuff.Length);
                int totalLength = BitConverter.ToInt32(totalLenBuff, 0);

                var buffer = new byte[totalLength];
                stream.Read(buffer, 0, buffer.Length);

                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    byte[] messageTypeHeader = new byte[4]; // to indentify the object
                    ms.Read(messageTypeHeader, 0, 4);

                    int objectType = BitConverter.ToInt16(messageTypeHeader, 0);

                    RemotingCommands messageType = (RemotingCommands)objectType;
                    Debug.WriteLine($"Received a message {messageType}");

                    switch (messageType)
                    {
                        case RemotingCommands.Unknown:
                            break;
                        case RemotingCommands.ConnectionRequest:
                            ConnectRequestMsg lMessage = ConnectRequestMsg.Parser.ParseFrom(ms);
                            // raise an event
                            
                            MessageRaised?.Invoke(null, new ExchangeMessage(lMessage, clientendPoint));
                            break;

                        default:
                            Console.WriteLine("Fatal error.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void Handle2(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}