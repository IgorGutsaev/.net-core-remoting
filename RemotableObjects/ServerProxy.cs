using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableObjects
{
    public class ServerProxy<TService>
    {
        private TService _Service;

        public ServerProxy(TService service)
        {
            this._Service = service;
        }
    }
}
