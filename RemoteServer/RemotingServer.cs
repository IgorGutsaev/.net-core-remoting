using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RemotableServer
{
    public class RemotingServer : IRemotingServer
    {
        private INetChannel _channel;

        public RemotingServer(INetChannel channel)
        {
            _channel = channel;
            _channel.OnChannelReport += (sender, message) => { Debug.WriteLine("Server: " + message); };
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
