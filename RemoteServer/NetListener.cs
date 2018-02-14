using RemotableInterfaces;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemotableServer
{
    public class NetListener : INetListener
    {
        private TcpListener _Listener;
        public INetServerSettings _serverSettings;
        private INetListenerHandler _listenerHandler;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private NetworkStream _NetworkStream = null;

        public NetListener(INetServerSettings serverSettings, INetListenerHandler listenerHandler)
        {
            this._serverSettings = serverSettings;
            this._listenerHandler = listenerHandler;
        }

        public void Start()
        {
            Task receive = Task.Run(() => {
                try
                {
                    _Listener = new TcpListener(this._serverSettings.GetServerAddress());
                    _Listener.Start();

                    Debug.WriteLine($"{DateTime.Now.ToString("T")} Server: listening {this._serverSettings.ToString()}");

                    while (!this.tokenSource.IsCancellationRequested)
                    {
                        using (TcpClient tcpClient = _Listener.AcceptTcpClient()) // Waiting for a client
                        {
                            Debug.WriteLine($"{DateTime.Now.ToString("T")} Server: request from {tcpClient.Client.RemoteEndPoint}");

                            using (_NetworkStream = tcpClient.GetStream())
                            {
                                this._listenerHandler.Process(_NetworkStream
                                    , (data) =>
                                    {
                                        tcpClient.Client.Send(data);
                                    });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    if (_Listener != null)
                        _Listener.Stop();
                }
            }, tokenSource.Token);
        }

        public void Stop()
        {
            this.tokenSource.Cancel();
        }
    }
}
