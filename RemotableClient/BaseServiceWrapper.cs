using RemotableInterface;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Diagnostics;

namespace RemotableClient
{
    internal abstract class BaseServiceWrapper
    {
        protected IClientProxy _proxy;

        public BaseServiceWrapper(IClientProxy proxy)
        {
            Debug.WriteLine($"Start service at {DateTime.Now.ToString("F")}");

            this._proxy = proxy;
            string responce = this.Connect();
        }

        private string Connect()
        {
            Debug.WriteLine($"Send connection request to {_proxy.GetServerEndpoint()}");

            ConnectRequestMsg message = new ConnectRequestMsg { Type = RemotingCommands.ConnectionRequest, Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()) };
            return this._proxy.Invoke(message).ToString();
        }
    }
}