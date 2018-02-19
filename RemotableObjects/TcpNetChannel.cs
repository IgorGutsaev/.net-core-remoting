using Google.Protobuf;
using ProtoBuf;
using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
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
        public event EventHandler<string> OnChannelReport;
        public event EventHandler<ServiceEvent> OnEvent;

        public class SendUnit
        {
            public IPEndPoint Endpoint;
            public NetPackage Pakage;
            public Action<object> Handler;
        }

        #region Variables
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private BlockingCollection<SendUnit> _PackageQueue = new BlockingCollection<SendUnit>(); // message queue 

        private INetServerSettings _serverSettings;
        private INetHandler _handler;
        private TcpListener _Listener;
        private NetworkStream _NetworkStream = null;
        #endregion

        public TcpNetChannel(INetServerSettings serverSettings, INetHandler handler)
        {
            this._serverSettings = serverSettings;
            this._handler = handler;
            this._handler.OnEventRaised += _handler_OnMessageRaised;
        }

        private void _handler_OnMessageRaised(ServiceEvent ev, IPEndPoint endpoint)
        {
            string type = ev.Data.GetType().IsGenericType ? ev.Data.GetType().FullName : ev.Data.GetType().AssemblyQualifiedName;

            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, ev.Data);

                TriggerEventMsg message =
                    new TriggerEventMsg { ServiceUid = ev.ServiceUid, Type = RemotingCommands.TriggerEvent, EventType = type, Value = ByteString.CopyFrom(ms.ToArray()) };

                this.Send(_handler.Pack(message), null, endpoint);
            }
        }

        public void Start()
        {
            IPEndPoint endpoint = this._serverSettings.GetServerAddress();

            _Listener = new TcpListener(endpoint.Address, !this.PortInUse(endpoint.Port) ? endpoint.Port : new Random().Next(65000, 65431));
            _Listener.Start();

            Thread.Sleep(100);

            this.Report($"Listening {_Listener.LocalEndpoint}");

            Task receive = Task.Run(() =>
            {
                try
                {
                    while (!this.tokenSource.IsCancellationRequested)
                    {
                        using (TcpClient tcpClient = _Listener.AcceptTcpClient()) // Waiting for a client
                        {
                            this.Report($"Port {((IPEndPoint)_Listener.LocalEndpoint).Port}: request from {tcpClient.Client.RemoteEndPoint}");

                            using (_NetworkStream = tcpClient.GetStream())
                            {
                                NetPackage response = this._handler.ProcessRequest(_NetworkStream, (ev) => { this.OnEvent(null, (ServiceEvent)ev); });
                                if (response != null)
                                    _NetworkStream.Write(response.Data, 0, response.Data.Length);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Report(ex.Message);
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
                    SendUnit unit = _PackageQueue.Take();
                    if (unit == null)
                        break;

                    using (TcpClient sClient = new TcpClient())
                    {
                        sClient.Connect(unit.Endpoint);
                        using (NetworkStream stream = sClient.GetStream())
                        {
                            stream.Write(unit.Pakage.Data, 0, unit.Pakage.Data.Length);
                            stream.ReadTimeout = 100 * 1000; // 100 sec

                            _handler.ProcessRequest(stream, unit.Handler);//, unit.Handler);
                        }
                    }
                }
            }, tokenSource.Token);
        }

        public bool PortInUse(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

            return tcpEndPoints.Any(p => p.Port == port);
        }

        public bool Connect()
        {
            this.Report($"Try connect to server");

            string response = this.Invoke(new ConnectRequestMsg { Type = RemotingCommands.ConnectionRequest }).ToString();

            this.Report($"Server answer: '{response}'");

            if (!String.Equals(response, "+ok", StringComparison.InvariantCultureIgnoreCase))
                throw new CommunicationException(response);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outgoingMessage">Message</param>
        /// <returns></returns>
        public object Invoke(object outgoingMessage)
        {
            object result = null;
            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);

            Action<object> handleResult = (incomeData) =>
            {
                result = incomeData;
                stopWaitHandle.Set();
            };

            this.Send(_handler.Pack(outgoingMessage), handleResult); //, handleMessage

            stopWaitHandle.WaitOne();

            if (result is Exception)
                throw (Exception)result;

            return result;
        }

        /// <summary>
        /// Send data to Listener
        /// </summary>
        /// <param name="package"></param>
        public void Send(NetPackage package, Action<object> handleResult = null, IPEndPoint endpoint = null)
        {
            if (package.Data.Length > 0)
                _PackageQueue.Add(new SendUnit { Pakage = package, Handler = handleResult, Endpoint = (endpoint == null ? this._serverSettings.GetServerAddress() : endpoint) });
        }

        public void Stop()
        {
            this.tokenSource.Cancel();
        }

        /// <summary>
        /// Listener address (for Client events e.g.)
        /// </summary>
        /// <returns></returns>
        public IPEndPoint GetCallbackAddress()
        {
            return (IPEndPoint)this._Listener.Server.LocalEndPoint;
        }

        private void Report(string message)
        { 
            this.OnChannelReport?.Invoke(this, message);
        }
    }
}