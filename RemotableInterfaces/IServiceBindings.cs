using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableInterfaces
{
    public interface IServiceBindings
    {
        string this[Type service] { get; }

        bool ContainsKey(Type key);

        IServiceBindings AddBinding<T>(string address);
    }
}
