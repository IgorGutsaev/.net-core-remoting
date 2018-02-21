using RemotableInterfaces;
using System;
using System.Collections.Generic;

namespace RemotableObjects
{
    public class ServiceBindings : Dictionary<Type, string>, IServiceBindings
    {
        public IServiceBindings AddBinding<T>(string address)
        {
            this.Add(typeof(T), address);
            return this;
        }
    }
}
