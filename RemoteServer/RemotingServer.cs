using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableServer
{
    public class RemotingServer : IRemotingServer
    {
        private INetChannel _channel;

        public RemotingServer(INetChannel channel)
        {
            _channel = channel;

            
        }

        public void Start()
        {
            _channel.Start();
        }

        public void Stop()
        {
            _channel.Stop(); 
        }
    }
}
