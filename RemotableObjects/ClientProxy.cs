using System;
using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;
using System.Threading;
using System.Net;
using System.Diagnostics;
using Google.Protobuf.Collections;
using System.Linq;
using System.IO;
using ProtoBuf;
using Google.Protobuf;

namespace RemotableObjects
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientProxy : IClientProxy
    {
        private string _ServiceProxyToken;
        private INetChannel _channel;
        private INetHandler _handler;

        public ClientProxy(INetChannel channel, INetHandler handler)
        {
            this._channel = channel;
            this._handler = handler;
            this._channel.Start(false);

            this.Connect();
        }

        /// <summary>
        /// Connect via channel to server
        /// </summary>
        private void Connect()
        {
            Debug.WriteLine($"{DateTime.Now.ToString("T")} Client: try connect to server");

            ConnectRequestMsg message =
                new ConnectRequestMsg { Type = RemotingCommands.ConnectionRequest };

            string response = this.Invoke<string>(message);

            Debug.WriteLine($"{DateTime.Now.ToString("T")} Client: server response is '{response}'");

            if (!String.Equals(response, "+ok", StringComparison.InvariantCultureIgnoreCase))
                throw new CommunicationException(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outgoingMessage">Message</param>
        /// <returns></returns>
        private T Invoke<T>(object outgoingMessage)
        {
            object result = null;
            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);

            Action<byte[]> handleMessage = (incomingMessage) =>
            {
                if (outgoingMessage is QueryInterfaceMsg)
                {
                    var t = QueryInterfaceResponseMsg.Parser.ParseFrom(incomingMessage);
                    result = t.InterfaceGuid;
                }
                else if (outgoingMessage is InvokeMethodMsg)
                {
                    var t = InvokeMethodResponseMsg.Parser.ParseFrom(incomingMessage);
                    using (var stream = new MemoryStream(t.Result.ToByteArray()))
                    {
                        result = Serializer.Deserialize(typeof(T), stream);
                    }
                }
                else if (outgoingMessage is ConnectRequestMsg)
                {
                    var t = ConnectResponseMsg.Parser.ParseFrom(incomingMessage);
                    result = t.Message;
                }
                else throw new CommunicationException("Client: Unknown message received!");

                stopWaitHandle.Set();
            };

            this._channel.Send(this._handler.Pack(outgoingMessage), handleMessage);
            stopWaitHandle.WaitOne();

            return (T)result;
        }

        public void BuildRemoteService(string interfaceName)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("T")} Client: ask the server to build '{interfaceName}' proxied service");

            QueryInterfaceMsg message =
                new QueryInterfaceMsg
                {
                    Type = RemotingCommands.QueryInterface,
                    InterfaceName = interfaceName
                };

            string serviceUid = this.Invoke<string>(message).ToString();
            if (String.IsNullOrWhiteSpace(serviceUid))
                throw new CommunicationException("Server cannot create a proxy!");

            this._ServiceProxyToken = serviceUid;
        }

        public T InvokeMethod<T>(string methodName, MethodParameterMsg[] parameters)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("T")} Client: invoke method '{methodName}' in proxied service");

            RepeatedField<MethodParameterMsg> repeatableParamsContainer =
                new RepeatedField<MethodParameterMsg>();
            repeatableParamsContainer.AddRange(parameters);
 
            InvokeMethodMsg message =
                new InvokeMethodMsg
                {
                    Type = RemotingCommands.InvokeMethod,
                    InterfaceGuid = this._ServiceProxyToken,
                    Method = methodName,
                     Parameters = { repeatableParamsContainer }
                };

            return this.Invoke<T>(message);
        }
    }
}
