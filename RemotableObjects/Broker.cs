using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RemotableObjects
{
    public class Broker : List<IServerProxy>, IBroker
    {
        private INetHandler _Handler;
        private INetChannel _Channel;
        private ServiceProvider Provider;

        public Broker(INetHandler handler, INetChannel channel)
        {
            this._Handler = handler;
            this._Channel = channel;
          //  this._Handler.OnMessageRaised += _Handler_OnMessageRaised;

            this.Provider = new ServiceCollection()
             .AddScoped<IMyService, MyService>()
             .BuildServiceProvider();

            this._Channel.Start(true);
        }

        //private void _Handler_OnMessageRaised(IMessage incomeMessage, Action<byte[]> onProcess)
        //{
        //    Action<IMessage> sendMessage = (responseMsg) =>
        //    {
        //        NetPackage package = this._SenderHandler.Pack(responseMsg);
        //        onProcess(package.Data);
        //    };

        //    IMessage responseMessage = null;

        //    try
        //    {
        //        if (incomeMessage is ConnectRequestMsg)
        //        {
        //            //throw new Exception("Trouble!");
        //            responseMessage = new ConnectResponseMsg { Message = "+ok", Type = RemotingCommands.ConnectionResponse };
        //        }
        //        else if (incomeMessage is QueryInterfaceMsg)
        //        {
        //            QueryInterfaceMsg queryMessage = (QueryInterfaceMsg)incomeMessage;
        //            responseMessage = this.CreateRemoteService(queryMessage.InterfaceName);
        //        }
        //        else if (incomeMessage is InvokeMethodMsg)
        //        {
        //            InvokeMethodMsg invokeMessage = (InvokeMethodMsg)incomeMessage;
        //            var service = this.FirstOrDefault(x=>x.GetUid() == invokeMessage.InterfaceGuid);
        //            if (service == null)
        //                throw new CommunicationException($"Service {invokeMessage.InterfaceGuid} not found!");

        //            #region Deserialize parameters // Move to helper
        //            List<MethodParameter> deserializedParameters = new List<MethodParameter>();

        //            MethodParameterMsg[] serializedParameters = invokeMessage.Parameters.ToArray();
        //            foreach (var p in serializedParameters)
        //            {
        //                using (var stream = new MemoryStream(p.Value.ToByteArray()))
        //                {
        //                    Type parType = Type.GetType(p.Type);
        //                    object variable = Serializer.Deserialize(parType, stream);
        //                    MethodParameter prm = new MethodParameter(p.Name, parType, variable);
        //                    deserializedParameters.Add(prm);
        //                }
        //            }
        //            #endregion

        //            object result = service.InvokeMethod(invokeMessage.Method, deserializedParameters);

        //            using (var stream = new MemoryStream())
        //            {
        //                Serializer.Serialize(stream, result);
        //                responseMessage = new InvokeMethodResponseMsg { Type = RemotingCommands.InvokeMethodResponse, Result = ByteString.CopyFrom(stream.ToArray()) };
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseMessage = new RemotingExceptionMsg
        //        {
        //            Message = ex.Message,
        //            Type = RemotingCommands.Exception
        //        };
        //    }

        //    if (responseMessage != null)
        //        sendMessage(responseMessage);
        //}

        private QueryInterfaceResponseMsg CreateRemoteService(string ServiceName)
        {
            string name = ServiceName.ToLowerInvariant().Trim();
            string serviceGuid = "";

            switch (name)
            {
                case "imyservice":
                    IMyService myService = this.Provider.GetRequiredService<IMyService>();
                    ServerProxy<IMyService> proxy = new ServerProxy<IMyService>(myService);
                    if (proxy.isCreated)
                        this.Add(proxy);

                    serviceGuid = proxy.Uid;
                    break;
                default:
                    throw new ArgumentException($"Cannot create Unknown service '{ServiceName}'!");
            }

            return new QueryInterfaceResponseMsg { InterfaceGuid = serviceGuid, Type = RemotingCommands.QueryInterfaceResponse };
        }
    }
}