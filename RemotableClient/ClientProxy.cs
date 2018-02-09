using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using RemotableServer;
using RemotableInterfaces;
using System.IO;
using RemotableInterface;

namespace RemotableClient
{
    /// <summary>
    /// 
    /// </summary>
    internal class ClientProxy : IClientProxy
    {
        public string ServiceUid { get; set; }
        public IPEndPoint Endpoint { get; set; }

        private INetChannel _channel;
        private INetSenderHandler _senderHandler;

        public ClientProxy(INetChannel channel, INetSenderHandler senderHandler)
        {
            this._channel = channel;
            this._senderHandler = senderHandler;
        }

        public object Invoke(object data)
        {
            this._channel.Send(this._senderHandler.Pack(this.ServiceUid, data));
           
            return "Some data";
        }
    }
}
