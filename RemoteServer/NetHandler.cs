using System.Net.Sockets;
using System.Net;
using System;
using RemoteCommunication.RemotableProtocol;
using System.Diagnostics;
using System.IO;
using Google.Protobuf;
using RemotableInterfaces;
using RemotableObjects;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace RemotableServer
{
    /// <summary>
    /// Stream processing
    /// </summary>
    public class NetHandler : INetHandler
    {
        public event onMessageRaised OnMessageRaised;

        /// <summary>
        /// Process 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="clientendPoint"></param>
        /// <param name="responseAction"></param>
        public void Process(Stream stream, Action<NetPackage> responseAction)
        {
            byte[] totalLenBuff = new byte[4];
            stream.Read(totalLenBuff, 0, totalLenBuff.Length);
            int totalLength = BitConverter.ToInt32(totalLenBuff, 0);

            var buffer = new byte[totalLength];
            stream.Read(buffer, 0, buffer.Length);

            IMessage responseMessage = null;

            try
            {
                using (MemoryStream incomeMs = new MemoryStream(buffer))
                {
                    byte[] messageTypeHeader = new byte[4]; // to indentify the object
                    incomeMs.Read(messageTypeHeader, 0, 4);

                    int objectType = BitConverter.ToInt16(messageTypeHeader, 0);

                    RemotingCommands messageType = (RemotingCommands)objectType;
                    Debug.WriteLine($"Received a message {messageType}");

                    switch (messageType)
                    {
                        case RemotingCommands.Unknown:
                            break;
                        case RemotingCommands.ConnectionRequest:
                            ConnectRequestMsg reqMsg = ConnectRequestMsg.Parser.ParseFrom(incomeMs);
                            responseMessage = new ConnectResponseMsg { Message = "+ok", Type = RemotingCommands.ConnectionResponse };
                            break;
                        case RemotingCommands.ConnectionResponse:
                            ConnectResponseMsg respMsg = ConnectResponseMsg.Parser.ParseFrom(incomeMs);
                            break;
                        case RemotingCommands.QueryInterface:
                            QueryInterfaceMsg interfaceMsg = QueryInterfaceMsg.Parser.ParseFrom(incomeMs);
                           //// responseMessage = this.CreateRemoteService(interfaceMsg.InterfaceName);
                            break;
                        case RemotingCommands.QueryInterfaceResponse:
                            QueryInterfaceResponseMsg interfaceRespMsg = QueryInterfaceResponseMsg.Parser.ParseFrom(incomeMs);
                            break;
                        case RemotingCommands.InvokeMethod:
                            {
                                InvokeMethodMsg invokeMethodMsg = InvokeMethodMsg.Parser.ParseFrom(incomeMs);
                                /*
                                var service = this.FirstOrDefault(x => x.GetUid() == invokeMessage.InterfaceGuid);
                                if (service == null)
                                    throw new CommunicationException($"Service {invokeMessage.InterfaceGuid} not found!");

                                #region Deserialize parameters // Move to helper
                                List<MethodParameter> deserializedParameters = new List<MethodParameter>();

                                MethodParameterMsg[] serializedParameters = invokeMessage.Parameters.ToArray();
                                foreach (var p in serializedParameters)
                                {
                                    using (var ms = new MemoryStream(p.Value.ToByteArray()))
                                    {
                                        Type parType = Type.GetType(p.Type);
                                        object variable = Serializer.Deserialize(parType, ms);
                                        MethodParameter prm = new MethodParameter(p.Name, parType, variable);
                                        deserializedParameters.Add(prm);
                                    }
                                }
                                #endregion

                                object result = service.InvokeMethod(invokeMessage.Method, deserializedParameters);

                                using (var ms = new MemoryStream())
                                {
                                    Serializer.Serialize(ms, result);
                                    responseMessage = new InvokeMethodResponseMsg { Type = RemotingCommands.InvokeMethodResponse, Result = ByteString.CopyFrom(ms.ToArray()) };
                                }
                                */
                                break;
                            }
                        case RemotingCommands.InvokeMethodResponse:
                            InvokeMethodResponseMsg invokeMethodRespMsg = InvokeMethodResponseMsg.Parser.ParseFrom(incomeMs);
                            break;
                        case RemotingCommands.Exception:
                            RemotingExceptionMsg msg = RemotingExceptionMsg.Parser.ParseFrom(incomeMs);
                            throw new Exception(msg.Message);
                        default:
                            Debug.WriteLine("Fatal error.");
                            throw new CommunicationException("Unknown messageType");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessage = new RemotingExceptionMsg
                {
                    Message = ex.Message,
                    Type = RemotingCommands.Exception
                };
            }

            if (responseMessage != null)
            {
                NetPackage package = this.Pack(responseMessage);
                responseAction(package);
            }
        }

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