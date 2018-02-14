using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableInterfaces
{
    public class MethodParameter
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public object Value { get; private set; }

        public MethodParameter(string name, Type type, object value)
        {
            this.Name = name;
            this.Type = type;
            this.Value = value;
        }
    }
}
