using RemotableInterfaces;
using RemotableInterfactes;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RemotableServer
{
    public class NetChannel : INetChannel
    {
        public EventHandler<string> ConnectionClosed;

        public string Uid = Guid.NewGuid().ToString();

        private bool keepAlive = true;
        private BlockingCollection<NetPackage> _PackageQueue = new BlockingCollection<NetPackage>(); // message queue 

        public INetServerEndpointSettings _settings;
        private Action<byte[], Action<byte[]>> _incomeDataHandler;

        /// <summary>
        /// For client
        /// </summary>
        /// <param name="settings"></param>
        public NetChannel(INetServerEndpointSettings settings, Action<byte[], Action<byte[]>> incomeDataHandler)
        {
            this._settings = settings;
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
        }

        private void LoopSend()
        {
            try
            {
                while (keepAlive)
                {
                    NetPackage package = _PackageQueue.Take();
                    if (package == null)
                        break;

                    using (TcpClient lClient = new TcpClient())
                    {
                        lClient.Connect(this._settings.ServerIpAddress, this._settings.ServerPortNumber);
                        using (NetworkStream stream = lClient.GetStream())
                        {
                            try
                            {
                                stream.Write(package.Data, 0, package.Data.Length);
                                stream.ReadTimeout = 100* 1000; // 100 sec
                                
                                byte[] data = new byte[512];
                                stream.Read(data, 0, data.Length);
                                this._incomeDataHandler?.Invoke(data, null);
                            }
                            catch (System.IO.IOException ex)
                            {
                                if (!keepAlive) Trace.WriteLine("user requested TcpClient shutdown.");
                                else Trace.WriteLine("disconnected");
                            }
                            catch (Exception ex) { Trace.WriteLine(ex.Message); }
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
}