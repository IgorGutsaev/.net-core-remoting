using RemotableInterface;
using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;
using System;

namespace RemotableClient
{
    class MyServiceWrapper : BaseServiceWrapper, IMyService
    {
        public MyServiceWrapper(IClientProxy clientProxy)
            :base(clientProxy)
        {
            QueryInterfaceMsg message = new QueryInterfaceMsg { Type = RemotingCommands.QueryInterface, InterfaceName = this.GetType().Name };
            object serviceCreateResponce = this._proxy.Invoke(message).ToString();
        }

        public void Do()
        {
            
        }
    }
}