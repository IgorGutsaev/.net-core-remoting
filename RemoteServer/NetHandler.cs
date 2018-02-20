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
using System.Net;

namespace RemotableServer
{
    public class NetHandler : INetHandler
    {
        private IBroker _broker;
        
        public string Id = Guid.NewGuid().ToString();

        public string Identifier { get; private set; } = "";

        public event onEventRaised OnEventRaised;

        public NetHandler(IBroker broker)
        {
            this._broker = broker;
            this._broker.OnEventRaised += (ev, endpoint) => { this.OnEventRaised?.Invoke(ev, endpoint); };
        }

        public NetPackage ProcessRequest(Stream stream, Action<object> handleResult)
        {
            Func<IMessage, NetPackage> Prepared = (message) => { return this.Pack(message); };

            IMessage income = null;

            try
            { 
                income = ProtobufMessageParser.GetMessage(stream); // throws server-side exceptions
                if (income != null)
                    Debug.WriteLine($"{this.Identifier} {this.Id}: '{income.GetType().Name}' received");
            }
            catch (Exception ex) {
                handleResult(ex); }

            try
            {
                if (income == null)
                    return null;
                else if (income is ConnectRequestMsg)
                    return Prepared(new ConnectResponseMsg { Type = RemotingCommands.ConnectionResponse, Message = "+ok" });
                if (income is ConnectResponseMsg)
                    handleResult(((ConnectResponseMsg)income).Message);
                else if (income is QueryInterfaceMsg)
                {
                    QueryInterfaceMsg response = (QueryInterfaceMsg)income;

                    string serviceGuid = _broker.CreateService(response.InterfaceName, new IPEndPoint(IPAddress.Parse(response.CallbackAddress), (int)response.CallbackPort));
                    return Prepared(new QueryInterfaceResponseMsg { Type = RemotingCommands.QueryInterfaceResponse, InterfaceGuid = serviceGuid });
                }
                else if (income is QueryInterfaceResponseMsg)
                {
                    string serviceGuid = ((QueryInterfaceResponseMsg)income).InterfaceGuid;
                    handleResult(serviceGuid);
                }
                else if (income is InvokeMethodMsg)
                {
                    InvokeMethodMsg message = (InvokeMethodMsg)income;

                    #region Deserialize parameters
                    List<MethodParameter> deserializedParameters = new List<MethodParameter>();

                    MethodParameterMsg[] serializedParameters = message.Parameters.ToArray();
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

                    object result = _broker.InvokeMethod(message.InterfaceGuid, message.Method, deserializedParameters) ?? "";

                    using (var ms = new MemoryStream())
                    {
                        Serializer.Serialize(ms, result);
                        return Prepared(new InvokeMethodResponseMsg { Type = RemotingCommands.InvokeMethodResponse, Result = ByteString.CopyFrom(ms.ToArray()), ResultType = result.GetType().AssemblyQualifiedName });
                    }
                }
                else if (income is InvokeMethodResponseMsg)
                {
                    InvokeMethodResponseMsg response = (InvokeMethodResponseMsg)income;

                    using (var ms = new MemoryStream(response.Result.ToByteArray()))
                    {
                        object result = Serializer.Deserialize(Type.GetType(response.ResultType), ms);
                        handleResult(result);
                    }
                }
                else if (income is TriggerEventMsg)
                {
                    TriggerEventMsg incomeEvent = (TriggerEventMsg)income;

                    using (var ms = new MemoryStream(incomeEvent.Value.ToByteArray()))
                    {
                        object result = Serializer.Deserialize(Type.GetType(incomeEvent.EventType), ms);
                        ServiceEvent ev = new ServiceEvent { ServiceUid = incomeEvent.ServiceUid, Data = result };
                        handleResult(ev);
                    }
                }
                else if (income is ReleaseInterfaceMsg)
                {
                    ReleaseInterfaceMsg releaseInterfaceMessage = (ReleaseInterfaceMsg)income;

                    _broker.ReleaseService(releaseInterfaceMessage.InterfaceUid);
                    return Prepared(new ReleaseInterfaceResponseMsg { Type = RemotingCommands.ReleaseInterfaceResponse });
                }
                else if (income is ReleaseInterfaceResponseMsg)
                {
                    ReleaseInterfaceResponseMsg releaseInterfaceResponseMessage = (ReleaseInterfaceResponseMsg)income;

                    handleResult("");
                }

                return null;
            }
            catch (Exception ex)
            {
                return Prepared(new RemotingExceptionMsg
                {
                    Message = ex.Message,
                    Type = RemotingCommands.Exception
                });
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

        public void SetHandlerIdentifier(string identifier)
        {
            if (this.Identifier.Length > 0)
                return;
            this.Identifier = identifier;
        }
    }
}