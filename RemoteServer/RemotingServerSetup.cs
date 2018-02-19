using System;
using System.Collections.Generic;

namespace RemotableServer
{
    public class RemotingServerSetup
    {
        internal IEnumerable<Type> PublishedServices => _publishedServices;

        private List<Type> _publishedServices = new List<Type>();

        public RemotingServerSetup PublishService<T>()
            where T : class
        {
            _publishedServices.Add(typeof(T));

            return this;
        }
    }
}