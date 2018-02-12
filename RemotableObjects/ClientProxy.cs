using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using RemotableServer;
using RemotableInterfaces;
using System.IO;
using RemotableInterface;

namespace RemotableObjects
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientProxy : IClientProxy
    {
        public string ServiceUid { get; set; }
        public IPEndPoint Server { get; set; }

        private INetChannel _channel;
        private INetSenderHandler _senderHandler;

        public ClientProxy(INetChannel channel, INetSenderHandler senderHandler)
        {
            this.ServiceUid = $"service-{Guid.NewGuid().ToString()}";

            this._channel = channel;
            this._senderHandler = senderHandler;

            this._channel.Start();
        }

        public object Invoke(object data)
        {
            this._channel.Send(this._senderHandler.Pack(this.ServiceUid, data));

            return "Some data";
        }

        public IPEndPoint GetServerEndpoint()
        {
            return this.Server;
        }
    }
}
