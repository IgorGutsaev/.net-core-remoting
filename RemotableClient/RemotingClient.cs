using System;
using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;
using System.Net;
using System.Diagnostics;
using Google.Protobuf.Collections;
using System.IO;
using ProtoBuf;
using System.Collections.Generic;
using RemotableClient;

namespace RemotableObjects
{
    /// <summary>
    /// 
    /// </summary>
    public class RemotingClient : IRemotingClient
    {
        public event EventHandler<ServiceEvent> OnEvent;

        private INetChannel _channel;
        private INetHandler _handler;
        private RemotingClientSetup _setup;

        //internal Dictionary<Type, string> _serviceCollection = new Dictionary<Type, string>(); // Singleton mode
        //public IDictionary<Type, string> ServiceCollection => _serviceCollection;

        internal Dictionary<string, Type> _serviceCollection = new Dictionary<string, Type>();
        public IDictionary<string, Type> ServiceCollection => _serviceCollection;

        public RemotingClient(INetServerSettings settings, INetChannel channel, INetHandler handler, RemotingClientSetup setup)
        {
            _channel = channel;
            _handler = handler;
            this._setup = setup;

            _channel.OnChannelReport += (sender, message) => { Debug.WriteLine($"Client: " + message); };
            _channel.Start(settings);
            _channel.Connect();
            _channel.OnEvent += (sender, ev) => { this.OnEvent?.Invoke(sender, ev); };
        }

        public string BuildRemoteService(Type interfaceType)
        {
            Debug.WriteLine($"Client: ask the server to build '{interfaceType.FullName}' proxied service");

            IPEndPoint myEndpoint = _channel.GetCallbackAddress();
            QueryInterfaceMsg message =
                new QueryInterfaceMsg
                {
                    Type = RemotingCommands.QueryInterface,
                    InterfaceName = interfaceType.AssemblyQualifiedName,
                    CallbackAddress = myEndpoint.Address.ToString(),
                    CallbackPort = (uint)myEndpoint.Port
                };

            IPEndPoint endpoint = this._setup.GetBinding(interfaceType).ToIPEndPoint();

            string serviceUid = _channel.Invoke(message, endpoint).ToString();

            if (String.IsNullOrWhiteSpace(serviceUid))
                throw new CommunicationException($"An error occurred while service {interfaceType.Name} initiating!");

            this.ServiceCollection.Add(serviceUid, interfaceType);

            return serviceUid;
        }

        public object InvokeMethod(string serviceUid, string methodName, MethodParameter[] parameters)
        {
            Debug.WriteLine($"Client: invoke method '{methodName}' in proxied service");

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
                    InterfaceGuid = serviceUid,
                    Method = methodName,
                    Parameters = { repeatableParamsContainer }
                };

            return this._channel.Invoke(message);
        }

        public void Dispose()
        {
            foreach (var service in ServiceCollection)
            {
                _channel.Invoke(new ReleaseInterfaceMsg
                {
                    Type = RemotingCommands.ReleaseInterface,
                    InterfaceUid = service.Key
                });
            }
        }
    }
}