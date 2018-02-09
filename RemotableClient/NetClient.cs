
using Google.Protobuf;
using RemotableInterfaces;
using RemotableInterfactes;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemotableClient
{
    public class NetworkClient
    {
        private INetClientSettings _settings;
        
        private CodedOutputStream _NetworkStreamOutput = null;
        private bool _ExitLoop = true;
        private BlockingCollection<object> _Queue = new BlockingCollection<object>(); // message queue 

        public NetworkClient(INetClientSettings settings)
        {
            this._settings = settings;
        }

        public void Connect()
        {
            if (!_ExitLoop) return; // running already
            _ExitLoop = false;

            Task.Run(() =>
            {
                Loop();
            });
        }

        public void Disconnect()
        {
            _ExitLoop = true;
            _Queue.Add(null);
            // if (_NetworkStream != null) _NetworkStream.ReadTimeout = 100;
        }

        public void Send(object xObject)
        {
            if (xObject == null) return;
            _Queue.Add(xObject);
        }

        private void Loop()
        {
            try
            {
                using (TcpClient lClient = new TcpClient())
                {
                    lClient.Connect(this._settings.ServerIpAddress, this._settings.ServerPortNumber);
                    using (_NetworkStreamOutput = new CodedOutputStream(lClient.GetStream()))
                    {

                        while (!_ExitLoop)
                        {
                            _ExitLoop = true;

                            try
                            {
                                object message = _Queue.Take();
                                ProtoType lObject = message.GetType().GetProperty("Type").GetValue(message, null) as ProtoType;
                                byte[] messageTypeID = BitConverter.GetBytes((Int16)lObject.Type);
                                if (lObject == null) break;

                                switch (lObject.Type)
                                {
                                    case RemotingCommands.QueryInterface:

                                        _NetworkStreamOutput.WriteBytes(ByteString.CopyFrom(messageTypeID));

                                        MyServiceMessageTypeA convertedMessage = (MyServiceMessageTypeA)message;
                                      //  int messageSize = convertedMessage.CalculateSize();
                                       // _NetworkStream.WriteBytes(ByteString.CopyFrom(BitConverter.GetBytes(messageSize)));
                                        convertedMessage.WriteTo(_NetworkStreamOutput);
                                       

                                        break;
                                    //case ProtoBufExample.eType.eFable:
                                    //    _NetworkStream.Write(lObject.objectIdAsBytes, 0, 2);
                                    //    ProtoBuf.Serializer.SerializeWithLengthPrefix<ProtoBufExample.Fable>(_NetworkStream, (ProtoBufExample.Fable)lObject, ProtoBuf.PrefixStyle.Fixed32);
                                    //    break;
                                    default:
                                        break;
                                }
                            }
                            catch (System.IO.IOException)
                            {
                                if (_ExitLoop) Console.WriteLine("user requested TcpClient shutdown.");
                                else Console.WriteLine("disconnected");
                            }
                            catch (Exception ex) { Console.WriteLine(ex.Message); }
                        }
                        _ExitLoop = true;
                        Console.WriteLine(Environment.NewLine + "client: shutting down");
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
}