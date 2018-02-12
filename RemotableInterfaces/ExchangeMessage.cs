using Google.Protobuf;
using System.Net;

namespace RemotableInterfaces
{
    public class ExchangeMessage
    {
        public EndPoint Endpoint;
        public IMessage Message;

        public ExchangeMessage(IMessage message, EndPoint endpoint)
        {
            this.Message = message;
            this.Endpoint = endpoint;
        }
    }
}
