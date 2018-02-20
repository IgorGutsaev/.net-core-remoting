using RemotableInterfaces;
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
            _channel.OnChannelReport += (sender, message) => { Debug.WriteLine("Server: " + message); };
            this._channel.SetHandlerIdentifier("ServerHandler");

        }

        public bool IsEnable()
        {
            return this._channel != null && this._channel.IsEnable();
        }

        public void Start()
        {
            _channel.Start(_settings);
        }

        public void Stop()
        {
            _channel.Stop(); 
        }
    }
}
