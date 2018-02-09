using System.IO;
using System.Net.Sockets;

namespace RemotableServer
{
    public interface INetListenerHandler
    {
        void Handle(Stream stream);
    }
}