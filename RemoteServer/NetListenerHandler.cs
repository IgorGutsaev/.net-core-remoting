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
    /// <summary>
    /// Stream processing
    /// </summary>
    public class NetListenerHandler : INetListenerHandler
    {
        public event onMessageRaised OnMessageRaised;

        /// <summary>
        /// Process 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="clientendPoint"></param>
        /// <param name="responseAction"></param>
        public void Process(Stream stream, Action<byte[]> responseAction)
        {
            byte[] totalLenBuff = new byte[4];
            stream.Read(totalLenBuff, 0, totalLenBuff.Length);
            int totalLength = BitConverter.ToInt32(totalLenBuff, 0);

            var buffer = new byte[totalLength];
            stream.Read(buffer, 0, buffer.Length);
            ProcessMessage(buffer, responseAction);
        }

        private void ProcessMessage(byte[] buffer, Action<byte[]> responseAction)
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
                    case RemotingCommands.QueryInterface:
                        message = QueryInterfaceMsg.Parser.ParseFrom(ms);
                        break;
                    case RemotingCommands.QueryInterfaceResponse:
                        message = QueryInterfaceResponseMsg.Parser.ParseFrom(ms);
                        break;
                    case RemotingCommands.InvokeMethod:
                        message = InvokeMethodMsg.Parser.ParseFrom(ms);
                        break;
                    case RemotingCommands.InvokeMethodResponse:
                        message = InvokeMethodResponseMsg.Parser.ParseFrom(ms);
                        break;
                    default:
                        Debug.WriteLine("Fatal error.");
                        break;
                }

                if (message != null && this.OnMessageRaised != null)
                    this.OnMessageRaised(message, (data) => { responseAction(data); });
            }
        }
    }
}