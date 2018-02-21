using RemotableInterfaces;
using RemotableObjects;
using System;
using System.Net;

namespace RemotableClient
{
    public sealed class RemotingClientSetup
    {
        private IServiceBindings _bindings;

        public IServiceBindings Bindings { get { return this._bindings; } }

        public RemotingClientSetup(IServiceBindings bindings)
        {
            this._bindings = bindings;
        }

        public IPEndPoint GetBinding(Type serviceType)
        {
            if (!this._bindings.ContainsKey(serviceType))
                throw new Exception($"No specified binding for service '{serviceType}'");

            return this._bindings[serviceType].ToIPEndPoint();
        }
    }
}