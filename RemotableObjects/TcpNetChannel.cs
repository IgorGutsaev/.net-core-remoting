using Google.Protobuf;
using ProtoBuf;
using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Concurrent;
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
        private CancellationToken ct;

        private BlockingCollection<SendUnit> _PackageQueue = new BlockingCollection<SendUnit>(); // message queue 

        private INetServerSettings _serverSettings;
        private INetHandler _handler;
        private TcpListener _Listener;
        private NetworkStream _NetworkStream = null;
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        #endregion

        public TcpNetChannel(INetHandler handler)
        {
            this._handler = handler;
            this._handler.OnEventRaised += _handler_OnMessageRaised;
            this.ct = tokenSource.Token;
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

        private void OnAccept(IAsyncResult iar)
        {
            TcpListener l = (TcpListener)iar.AsyncState;

            if (l == null)
                return;

            TcpClient c;
            try
            {
                c = l.EndAcceptTcpClient(iar);

                using (_NetworkStream = c.GetStream())
                {
                    NetPackage response = this._handler.ProcessRequest(_NetworkStream, (ev) => { this.OnEvent(this, (ServiceEvent)ev); });
                    if (response != null)
                        _NetworkStream.Write(response.Data, 0, response.Data.Length);
                }



                // keep listening
                l.BeginAcceptTcpClient(new AsyncCallback(OnAccept), l);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error accepting TCP connection: {0}", ex.Message);

                // unrecoverable
                ////  _doneEvent.Set();
                return;
            }
            catch (ObjectDisposedException)
            {
                // The listener was Stop()'d, disposing the underlying socket and
                // triggering the completion of the callback. We're already exiting,
                // so just return.
                Console.WriteLine("Listen canceled.");
                return;
            }
            catch (Exception ex)
            {
                return;
            }
   


            // meanwhile...
            //SslStream s = new SslStream(c.GetStream());
            //Console.WriteLine("Authenticating...");
            //s.BeginAuthenticateAsServer(_cert, new AsyncCallback(OnAuthenticate), s);
        }


        public void Start(INetServerSettings serverSettings)
        {
            this._serverSettings = serverSettings;
            IPEndPoint endpoint = this._serverSettings.GetServerAddress();
            
            _Listener = new TcpListener(endpoint.Address, endpoint.Port);
            _Listener.Start();

            Thread.Sleep(100);

            this.Report($"Listening {_Listener.LocalEndpoint}");

            _Listener.BeginAcceptTcpClient(new AsyncCallback(OnAccept), _Listener);

            //Task receive = Task.Run(() =>
            //{
            //    try
            //    {
            //        while (!ct.IsCancellationRequested)
            //        {
            //            using (TcpClient tcpClient = _Listener.AcceptTcpClient()) // Waiting for a client
            //            {
            //                using (_NetworkStream = tcpClient.GetStream())
            //                {
            //                    NetPackage response = this._handler.ProcessRequest(_NetworkStream, (ev) => { this.OnEvent(this, (ServiceEvent)ev); });
            //                    if (response != null)
            //                        _NetworkStream.Write(response.Data, 0, response.Data.Length);
            //                }
            //            }
            //        }
            //    }
            //    catch (SocketException e) when (e.SocketErrorCode == SocketError.Interrupted)
            //    {
            //       // throw new OperationCanceledExeption();
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Report(ex.Message);
            //    }
            //}, ct);


            Task send = Task.Run(() =>
            {
                while (!this.tokenSource.IsCancellationRequested)
                {
                    SendUnit unit = _PackageQueue.Take();
                    if (unit == null)
                        break;

                    using (TcpClient sClient = new TcpClient())
                    {
                        //sClient.ReceiveTimeout
                        sClient.Connect(unit.Endpoint);
                        using (NetworkStream stream = sClient.GetStream())
                        {
                            stream.Write(unit.Pakage.Data, 0, unit.Pakage.Data.Length);
                            stream.ReadTimeout = 100 * 1000; // 100 sec

                            _handler.ProcessRequest(stream, unit.Handler);//, unit.Handler);
                        }
                    }
                }
            }, ct);
        }

        public bool PortInUse(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

            return tcpEndPoints.Any(p => p.Port == port);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outgoingMessage">Message</param>
        /// <returns></returns>
        public object Invoke(object outgoingMessage, IPEndPoint endpoint = null)
        {
            object result = null;
            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);

            Action<object> handleResult = (incomeData) =>
            {
                result = incomeData;
                stopWaitHandle.Set();
            };

            this.Send(_handler.Pack(outgoingMessage), handleResult, endpoint); //, handleMessage

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

            if (_Listener != null)
            {
                _Listener.Stop();
                _Listener.Server.Close();
                while (_Listener.Server.Connected)
                    Thread.Sleep(50);
            }
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
            this.OnChannelReport?.Invoke(this, $"{message} [Channel {this.Id}]");
        }

        public void SetHandlerIdentifier(string identifier)
        {
            this._handler.SetHandlerIdentifier(identifier);
        }

        public bool IsEnable()
        {
            try
            {
                int read = this._Listener.Server.Available;

                return true;
            }
            catch (ObjectDisposedException ex)
            {
                // SocketClosed;
                return false;
            }
        }
    }
}