using RemotableInterface;
using RemotableInterfaces;
using System;

namespace RemotableClient
{
    class MyServiceWrapper : BaseServiceWrapper, IMyService
    {
        public MyServiceWrapper(IClientProxy clientProxy)
            :base(clientProxy)
        {

        }

        public void Do()
        {
            throw new NotImplementedException();
        }
    }
}