using Google.Protobuf;
using RemotableInterfaces;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemotableObjects
{
    /// <summary>
    /// Client-side send-receive tool
    /// </summary>
    public class TcpNetChannel : INetChannel
    {
        public class SendUnit
        {
            public IPEndPoint Endpoint;
            public NetPackage Pakage;
            public Action<byte[]> Handler;
        }

        #region Variables
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private BlockingCollection<SendUnit> _PackageQueue = new BlockingCollection<SendUnit>(); // message queue 

        private INetServerSettings _serverSettings;
        private INetHandler _handler;
        private TcpClient _TcpClient;
        private TcpListener _Listener;
        private NetworkStream _NetworkStream = null;
        #endregion

        public TcpNetChannel(INetServerSettings serverSettings, INetHandler handler)
        {
            this._serverSettings = serverSettings;
            this._handler = handler;
            this._handler.OnMessageRaised += _Handler_OnMessageRaised;
        }

        private void _Handler_OnMessageRaised(IMessage message, Action<byte[]> onProcess)
        {
            onProcess(message.ToByteArray());
        }

        public void Start(bool isServer)
        {
            this._TcpClient = new TcpClient();

            IPEndPoint endpoint = this._serverSettings.GetServerAddress();

            _Listener = new TcpListener(endpoint.Address, !this.PortInUse(endpoint.Port) ? endpoint.Port : new Random().Next(65000, 65431));
            _Listener.Start();

            Thread.Sleep(500);
           
            Debug.WriteLine($"Create tcp client");
            Debug.WriteLine($"Listening {_Listener.LocalEndpoint}");

            Task receive = Task.Run(() => {
                try
                {
                    while (!this.tokenSource.IsCancellationRequested)
                    {
                        using (TcpClient tcpClient = _Listener.AcceptTcpClient()) // Waiting for a client
                        {
                            Debug.WriteLine($"{DateTime.Now.ToString("T")} Server: request from {tcpClient.Client.RemoteEndPoint}");

                            using (_NetworkStream = tcpClient.GetStream())
                            {
                                if (isServer)
                                    this._handler.Process(_NetworkStream
                                        , (data) =>
                                        {
                                            _NetworkStream.Write(data.Data, 0, data.Data.Length);
                                            //this.Send(data, null, (IPEndPoint)tcpClient.Client.RemoteEndPoint);
                                        });
                                else this._handler.Process(_NetworkStream, null);
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

            Task send = Task.Run(() =>
            {
                while (!this.tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        SendUnit unit = _PackageQueue.Take();
                        if (unit == null)
                            break;

                        this._TcpClient.Connect(unit.Endpoint);
                        using (NetworkStream stream = this._TcpClient.GetStream())
                        {
                            stream.Write(unit.Pakage.Data, 0, unit.Pakage.Data.Length);
                            stream.ReadTimeout = 100 * 1000; // 100 sec

                            if (!isServer)
                                this._handler.Process(stream, null);
                        }
                    }
                    catch (Exception ex) { Trace.WriteLine($"{DateTime.Now.ToString("T")} {ex.Message}"); }
                    //  finally { this._TcpClient.Close(); }
                }
            }, tokenSource.Token);
        }

        public bool PortInUse(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

            return tcpEndPoints.Any(p => p.Port == port);
        }

        /// <summary>
        /// Send data to Listener
        /// </summary>
        /// <param name="package"></param>
        public void Send(NetPackage package, Action<byte[]> handler, IPEndPoint endpoint = null)
        {
            
            if (package.Data.Length > 0)
                _PackageQueue.Add(new SendUnit { Pakage = package, Handler = handler, Endpoint = (endpoint == null ? this._serverSettings.GetServerAddress() : endpoint) });
        }

        public void Disconnect()
        {
            this.tokenSource.Cancel();
        }
    }
}