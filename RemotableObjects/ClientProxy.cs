using System;
using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;
using System.Net;
using System.Diagnostics;
using Google.Protobuf.Collections;
using System.IO;
using ProtoBuf;
using System.Collections.Generic;

namespace RemotableObjects
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientProxy : IClientProxy
    {
        public event EventHandler<ServiceEvent> OnEvent;

        private string _ServiceProxyToken;
        private INetChannel _channel;
        private INetHandler _handler;

        public ClientProxy(INetChannel channel, INetHandler handler)
        {
            _channel = channel;
            _handler = handler;
            _channel.Start();
            _channel.Connect();
            _channel.OnEvent += (sender, ev) => { this.OnEvent?.Invoke(sender, ev); };
        }

        public void BuildRemoteService(Type interfaceType)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("T")} Client: ask the server to build '{interfaceType.FullName}' proxied service");

            IPEndPoint myEndpoint = _channel.GetCallbackAddress();
            QueryInterfaceMsg message =
                new QueryInterfaceMsg
                {
                    Type = RemotingCommands.QueryInterface,
                    InterfaceName = interfaceType.AssemblyQualifiedName,
                    CallbackAddress = myEndpoint.Address.ToString(),
                    CallbackPort = (uint)myEndpoint.Port
                };

            string serviceUid = _channel.Invoke(message).ToString();

            if (String.IsNullOrWhiteSpace(serviceUid))
                throw new CommunicationException($"An error occurred while service {interfaceType.Name} initiating!");

            this._ServiceProxyToken = serviceUid;
        }

        public object InvokeMethod(string methodName, MethodParameter[] parameters)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("T")} Client: invoke method '{methodName}' in proxied service");

            RepeatedField<MethodParameterMsg> repeatableParamsContainer =
                new RepeatedField<MethodParameterMsg>();

            List<MethodParameterMsg> msgParameters = new List<MethodParameterMsg>();

            foreach (var p in parameters)
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, p.Value);
                    stream.Position = 0;

                    MethodParameterMsg param = new MethodParameterMsg() { Name = p.Name, Type = (p.Type.IsGenericType ? p.Type.FullName : p.Type.AssemblyQualifiedName), Value = Google.Protobuf.ByteString.FromStream(stream) };
                    msgParameters.Add(param);
                }
            }

            repeatableParamsContainer.AddRange(msgParameters);

            InvokeMethodMsg message =
                new InvokeMethodMsg
                {
                    Type = RemotingCommands.InvokeMethod,
                    InterfaceGuid = this._ServiceProxyToken,
                    Method = methodName,
                    Parameters = { repeatableParamsContainer }
                };

            return this._channel.Invoke(message);
        }

        public void Dispose()
        {
            _channel.Invoke(new ReleaseInterfaceMsg
            {
                Type = RemotingCommands.ReleaseInterface,
                InterfaceUid = _ServiceProxyToken
            });
        }
    }
}