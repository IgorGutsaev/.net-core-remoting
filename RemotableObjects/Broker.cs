using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RemotableObjects
{
    public class Broker : List<IServerProxy>, IBroker
    {
        private INetListenerHandler _Handler;
        private INetChannelListener _Listener;
        private INetSenderHandler _SenderHandler;
        private ServiceProvider Provider;

        public Broker(INetListenerHandler handler, INetChannelListener listener, INetSenderHandler senderHandler)
        {
            this._Handler = handler;
            this._Listener = listener;
            this._SenderHandler = senderHandler;
            this._Handler.MessageRaised += Handler_MessageRaised;

            this.Provider = new ServiceCollection()
             .AddScoped<IMyService, MyService>()
             .BuildServiceProvider();

            this._Listener.Start();
        }

        public string CreateProxy(string ServiceName)
        {
            string name = ServiceName.ToLowerInvariant().Trim();

            switch (name)
            {
                case "myservice":
                    IMyService myService = this.Provider.GetRequiredService<IMyService>();
                    ServerProxy<IMyService> proxy = new ServerProxy<IMyService>(myService);
                    return proxy.Uid;
                default:
                    throw new ArgumentException($"Cannot create Unknown service '{ServiceName}'!");
            }
        }

        private void Handler_MessageRaised(object sender, ExchangeMessage e)
        {
            if (e.Message is ConnectRequestMsg)
            {
             
                     ////   var responce = new ConnectResponseMsg { Message = "+ok", Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime( DateTime.Now.ToUniversalTime()), Type = RemotingCommands.ConnectionResponse };
                     ////   var package = this._SenderHandler.Pack("", responce);
                     ////   stream.Write(package.Data, 0, package.Data.Length);
 
            }
        }
    }
}
