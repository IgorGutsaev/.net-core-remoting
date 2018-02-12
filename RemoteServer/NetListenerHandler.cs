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
                ProcessMessage(clientendPoint, buffer);
            }
            catch (Exception ex)
            {

            }
        }

        public void Handle2(byte[] data)
        {
            try
            {
                int totalLength = BitConverter.ToInt32(data, 0);

                var buffer = new byte[totalLength];

                Array.Copy(data, 4, buffer, 0, totalLength);

                this.ProcessMessage(null, buffer);

            }
            catch { }
        }

        private void ProcessMessage(EndPoint clientendPoint, byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                byte[] messageTypeHeader = new byte[4]; // to indentify the object
                ms.Read(messageTypeHeader, 0, 4);

                int objectType = BitConverter.ToInt16(messageTypeHeader, 0);

                RemotingCommands messageType = (RemotingCommands)objectType;
                Debug.WriteLine($"Received a message {messageType}");

                IMessage message = null;

                switch (messageType)
                {
                    case RemotingCommands.Unknown:
                        break;
                    case RemotingCommands.ConnectionRequest:
                        message = ConnectRequestMsg.Parser.ParseFrom(ms);
                        break;
                    case RemotingCommands.ConnectionResponse:
                        message = ConnectResponseMsg.Parser.ParseFrom(ms);
                        break;
                    default:
                        Console.WriteLine("Fatal error.");
                        break;
                }

                if (message != null)
                    MessageRaised?.Invoke(null, new ExchangeMessage(message, clientendPoint));
            }
        }


    }
}