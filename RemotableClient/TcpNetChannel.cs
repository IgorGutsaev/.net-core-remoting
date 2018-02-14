using Google.Protobuf;
using RemotableInterfaces;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemotableClient
{
    /// <summary>
    /// Client-side send-receive tool
    /// </summary>
    public class TcpNetChannel : INetChannel
    {
        public class SendUnit
        {
            public NetPackage Pakage;
            public Action<byte[]> Handler;
        }

        #region Variables
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private BlockingCollection<SendUnit> _PackageQueue = new BlockingCollection<SendUnit>(); // message queue 

        private INetServerSettings _serverSettings;
        private INetListenerHandler _handler;
        #endregion

        public TcpNetChannel(INetServerSettings serverSettings, INetListenerHandler handler)
        {
            this._serverSettings = serverSettings;
            this._handler = handler;
            this._handler.OnMessageRaised += _Handler_OnMessageRaised;
        }

        private void _Handler_OnMessageRaised(IMessage message, Action<byte[]> onProcess)
        {
            onProcess(message.ToByteArray());
        }

        public void Start()
        {
            Task send = Task.Run(() =>
            {
                while (!this.tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        SendUnit unit = _PackageQueue.Take();
                        if (unit == null)
                            break;

                        using (TcpClient lClient = new TcpClient())
                        {
                            lClient.Connect(this._serverSettings.GetServerAddress());
                            using (NetworkStream stream = lClient.GetStream())
                            {
                                stream.Write(unit.Pakage.Data, 0, unit.Pakage.Data.Length);
                                stream.ReadTimeout = 100 * 1000; // 100 sec

                                this._handler.Process(stream, unit.Handler);
                            }
                        }
                    }
                    catch (Exception ex) { Trace.WriteLine($"{DateTime.Now.ToString("T")} Client: {ex.Message}"); }
                }
            }, tokenSource.Token);
        }

        /// <summary>
        /// Send data to Listener
        /// </summary>
        /// <param name="package"></param>
        public void Send(NetPackage package, Action<byte[]> handler)
        {
            if (package.Data.Length > 0)
                _PackageQueue.Add(new SendUnit { Pakage = package, Handler = handler });
        }

        public void Disconnect()
        {
            this.tokenSource.Cancel();
        }
    }
}