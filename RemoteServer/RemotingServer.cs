using RemotableInterfaces;
using System;
using System.Diagnostics;

namespace RemotableServer
{
    public class RemotingServer : IRemotingServer
    {
        private INetChannel _channel;
        private INetServerSettings _settings;

        public RemotingServer(INetServerSettings settings, INetChannel channel)
        {
            _channel = channel;
            _settings = settings;
            _channel.OnChannelReport += (sender, message) => { Console.WriteLine(message);  Debug.WriteLine("Server: " + message); };
        }

        public void Dispose()
        {
              _channel.Stop();
        }

        public bool IsEnable()
        {
            return this._channel != null && this._channel.IsEnable();
        }

        public void Start() => _channel.Start(_settings);
    }
}
