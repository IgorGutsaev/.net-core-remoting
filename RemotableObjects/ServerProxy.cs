using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableObjects
{
    public class ServerProxy<TService> : IServerProxy
    {
        public string Uid { get; private set; } = Guid.NewGuid().ToString();
        private TService _Service;

        public ServerProxy(TService service)
        {
            this._Service = service;
        }
    }
}
