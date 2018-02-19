using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableClient
{
    public sealed class RemotingClientSetup
    {
        public RemotingClientSetup()
        {
            this.BindRemoteService<IMyService>("127.0.0.1:65432");
        }
        
        public RemotingClientSetup BindRemoteService<T>(String endpointUri)
        {
            _bindings[typeof(T)] = endpointUri;

            return this;
        }

        internal IDictionary<Type, String> Bindings => _bindings;

        private Dictionary<Type, String> _bindings = new Dictionary<Type, String>();

        public string GetBinding(Type serviceType)
        {
            if (!this._bindings.ContainsKey(serviceType))
                throw new Exception($"No specified binding for service '{serviceType}'");

            return this._bindings[serviceType];
        }
    }
}
