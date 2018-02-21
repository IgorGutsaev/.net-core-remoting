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
using System.Linq;

namespace RemotableObjects
{
    /// <summary>
    /// Client communication tool
    /// Check bindings, initializ/release services, invoke methods, handle events 
    /// </summary>
    public class RemotingClient : IRemotingClient
    {
        public event EventHandler<ServiceEvent> OnEvent;

        private INetChannel _channel;
        private RemotingClientSetup _setup;

        internal Dictionary<string, Type> _serviceCollection = new Dictionary<string, Type>();
        public IDictionary<string, Type> ServiceCollection { get { return _serviceCollection; } }

        public RemotingClient(INetServerSettings settings, INetChannel channel, RemotingClientSetup setup)
        {
            _channel = channel;
            this._setup = setup;

            _channel.OnChannelReport += (sender, message) => { Debug.WriteLine($"Client: " + message); };
            _channel.Start(settings);
            _channel.OnEvent += (sender, ev) => { this.OnEvent?.Invoke(sender, ev); };
        }

        public string InvokeService(Type interfaceType)
        {
            this.CheckBinding(interfaceType);
   
            IPEndPoint myEndpoint = _channel.GetCallbackAddress();
            QueryInterfaceMsg message =
                new QueryInterfaceMsg
                {
                    Type = RemotingCommands.QueryInterface,
                    InterfaceName = interfaceType.AssemblyQualifiedName,
                    CallbackAddress = myEndpoint.Address.ToString(),
                    CallbackPort = (uint)myEndpoint.Port
                };

            IPEndPoint endpoint = this._setup.GetBinding(interfaceType);

            string serviceUid = _channel.Invoke(message, endpoint).ToString();

            if (String.IsNullOrWhiteSpace(serviceUid))
                throw new CommunicationException($"An error occurred while service {interfaceType.Name} initiating!");

            this.ServiceCollection.Add(serviceUid, interfaceType);

            return serviceUid;
        }

        public object InvokeMethod(string serviceUid, string methodName, MethodParameter[] parameters)
        {
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

            Type serviceType = this.ServiceCollection[serviceUid];
            IPEndPoint endpoint = this._setup.GetBinding(serviceType);

            return this._channel.Invoke(message, endpoint);
        }

        public void Dispose()
        {
            // Released created services on server side
            foreach (var service in ServiceCollection)
            {
                Type serviceType = service.Value;
                IPEndPoint endpoint = this._setup.GetBinding(serviceType);

                _channel.Invoke(new ReleaseInterfaceMsg
                {
                    Type = RemotingCommands.ReleaseInterface,
                    InterfaceUid = service.Key
                }, endpoint);
            }

            _channel.Stop();
        }

        private void CheckBinding(Type interfaceType)
        {
            IPEndPoint endpoint = _setup.Bindings[interfaceType]?.ToIPEndPoint();

            if (endpoint == null)
                throw new NotSupportedException($"There is no binding for '{interfaceType}'");

            string response = this._channel.Invoke(new ConnectRequestMsg { Type = RemotingCommands.ConnectionRequest }, endpoint).ToString();

            if (!String.Equals(response, "+ok", StringComparison.InvariantCultureIgnoreCase))
                throw new CommunicationException(response);

        }
    }
}