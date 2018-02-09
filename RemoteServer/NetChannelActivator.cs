using RemotableInterfaces;
using RemotableInterfactes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemotableServer
{
    public class NetChannelActivator : INetChannelActivator
    {
        private bool keepAlive = true;
        private TcpListener _Server;
        public INetServerSettings _Settings;

        private List<NetChannel> _Channels = new List<NetChannel>();
        private Action<Stream> _IncomeDataHandler;

        public NetChannelActivator(INetServerSettings settings, Action<Stream> incomeDataHandler)
        {
            this._Settings = settings;
            this._IncomeDataHandler = incomeDataHandler;
        }

        public void Start()
        {
            _Server = new TcpListener(this._Settings.ServerIpAddress, this._Settings.ServerPortNumber);
            _Server.Start();

            Task receive = Task.Run(() => LoopReceive());
        }

        public void Stop()
        {
            this.keepAlive = false;
            this._Server.Stop();
        }

        private void LoopReceive()
        {
            try
            {
                while (keepAlive)
                {
                    using (TcpClient tcpClient = _Server.AcceptTcpClient()) // Waiting for a client
                    {
                        NetChannel newChannel = new NetChannel(tcpClient, _IncomeDataHandler);
                        newChannel.ConnectionClosed += (sender, channelUid) => { Debug.WriteLine($"Channel '{channelUid}' is closed."); this._Channels.RemoveAll(c => c.Uid == channelUid); };
                        this._Channels.Add(newChannel);
                        newChannel.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                keepAlive = false;
                if (_Server != null) _Server.Stop();
            }
        }
    }
}
