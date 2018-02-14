using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableObjects
{
    public class CommunicationException : Exception
    {
        public CommunicationException(string message)
            : base(message)
        {

        }
    }
}
