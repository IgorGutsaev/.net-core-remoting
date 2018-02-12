using Microsoft.Extensions.DependencyInjection;
using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemotableObjects
{
    public class Broker : List<IServerProxy>, IBroker
    {
        private INetListenerHandler _Handler;
        private INetListener _Listener;
        private INetSenderHandler _SenderHandler;
        private ServiceProvider Provider;

        public Broker(INetListenerHandler handler, INetListener listener, INetSenderHandler senderHandler)
        {
            this._Handler = handler;
            this._Listener = listener;
            this._SenderHandler = senderHandler;
            this._Handler.OnMessageRaised += _Handler_OnMessageRaised;

            this.Provider = new ServiceCollection()
             .AddScoped<IMyService, MyService>()
             .BuildServiceProvider();

            this._Listener.Start();
        }

        private void _Handler_OnMessageRaised(ExchangeMessage message, Action<byte[]> onProcess)
        {
            if (message.Message is ConnectRequestMsg)
            {
                var response = new ConnectResponseMsg { Message = "+ok", Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()), Type = RemotingCommands.ConnectionResponse };
                var package = this._SenderHandler.Pack("", response);

                onProcess(package.Data);
            }
            else if (message.Message is QueryInterfaceMsg)
            {
                QueryInterfaceMsg queryMessage = (QueryInterfaceMsg)message.Message;
                this.CreateProxy(queryMessage.InterfaceName);

                onProcess(new byte[0]); // Query interface didn't returns any data.
            }
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


    }
}
