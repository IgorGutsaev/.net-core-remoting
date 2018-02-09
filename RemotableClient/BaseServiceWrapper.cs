using RemotableInterface;
using RemoteCommunication.RemotableProtocol;
using System;

namespace RemotableClient
{
    internal abstract class BaseServiceWrapper
    {
        private IClientProxy _proxy;

        public BaseServiceWrapper(IClientProxy proxy)
        {
            this._proxy = proxy;
            this.Connect();
        }

        private string Connect()
        {
            ConnectRequestMsg message = new ConnectRequestMsg { Type = RemotingCommands.ConnectionRequest, Address = "localhost", Port = 65432, Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()) };
            return this._proxy.Invoke(message).ToString();
        }
    }
}