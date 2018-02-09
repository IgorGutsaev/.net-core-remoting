using RemotableInterfaces;
using RemotableInterfactes;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RemotableServer
{
    public class NetChannel : INetChannel
    {
        public EventHandler<string> ConnectionClosed;

        public string Uid = Guid.NewGuid().ToString();
        private TcpClient _Client;

        private bool keepAlive = true;
        private BlockingCollection<NetPackage> _PackageQueue = new BlockingCollection<NetPackage>(); // message queue 

        public INetServerSettings _settings;
        private Action<Stream> _incomeDataHandler;

        /// <summary>
        /// For client
        /// </summary>
        /// <param name="settings"></param>
        public NetChannel(INetServerSettings settings)
        {
            this._settings = settings;
            this._Client = new TcpClient(_settings.ServerIpAddress.ToString(), _settings.ServerPortNumber);
            var ok = this._Client.Connected;
        }

        /// <summary>
        /// For server
        /// </summary>
        /// <param name="client"></param>
        /// <param name="incomeDataHandler"></param>
        public NetChannel(TcpClient client, Action<Stream> incomeDataHandler)
        {
            this._Client = client;
            this._incomeDataHandler = incomeDataHandler;
        }

        public bool Start()
        {
            try
            {
                Task send = Task.Run(() => LoopSend());

                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return false;
        }

        /// <summary>
        /// Send data to Client
        /// </summary>
        /// <param name="package"></param>
        public void Send(NetPackage package)
        {
            if (!String.IsNullOrWhiteSpace(package.serviceUid) && package.Data.Length > 0)
                _PackageQueue.Add(package);
        }

        public void Disconnect()
        {
            this.ConnectionClosed?.Invoke(this, null);
            keepAlive = false;
            this._Client.Close();
        }

        private void LoopSend()
        {
            try
            {
                while (keepAlive)
                {
                    if (!this._Client.Connected)
                    {
                        keepAlive = false;
                        this.ConnectionClosed?.Invoke(this, this.Uid);
                        continue;
                    }

                    using (NetworkStream stream = this._Client.GetStream())
                    {
                        try
                        {
                            _incomeDataHandler(stream);
                            ////stream.Write(result, 0, result.Length);
                            NetPackage package = _PackageQueue.Take();
                            if (package == null)
                                break;

                            stream.Write(package.Data, 0, package.Data.Length);
                        }
                        catch (System.IO.IOException)
                        {
                            if (!keepAlive) Trace.WriteLine("user requested TcpClient shutdown.");
                            else Trace.WriteLine("disconnected");
                        }
                        catch (Exception ex) { Trace.WriteLine(ex.Message); }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
}