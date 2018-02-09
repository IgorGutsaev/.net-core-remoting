using System.Net.Sockets;
using System.Net;
using System;
using RemoteCommunication.RemotableProtocol;
using System.Diagnostics;
using System.IO;

namespace RemotableServer
{
    public class NetListenerHandler : INetListenerHandler
    {
        public event EventHandler<ConnectRequestMsg> ConnectRaised;

        public NetListenerHandler()
        {
            

        }

        public void Handle(Stream stream)
        {
            int messageTypeLength = (int)stream.ReadByte(); // first byte is messagetype data length

            byte[] messageTypeHeader = new byte[2]; // to indentify the object
            stream.Read(messageTypeHeader, 0, messageTypeLength);

            int objectType = BitConverter.ToInt16(messageTypeHeader, 0);

            RemotingCommands messageType = (RemotingCommands)objectType;
            switch (messageType)
            {
                case RemotingCommands.Unknown:
                    break;
                case RemotingCommands.ConnectionRequest:
                    ConnectRequestMsg lMessage = ConnectRequestMsg.Parser.ParseFrom(stream);
                    Debug.WriteLine($"Received a message {lMessage.Type}");

                    // raise an event
                    ConnectRaised?.Invoke(null, lMessage);
                    break;

                default:
                    Console.WriteLine("Fatal error.");
                    break;
            }
        }
    }
}