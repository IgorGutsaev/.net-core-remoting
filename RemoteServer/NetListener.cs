using RemotableInterfaces;
using RemotableInterfactes;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RemotableServer
{
    public class NetListener : INetListener
    {
        private bool keepAlive = true;
        private TcpListener _Listener;
        public INetServerEndpointSettings _serverAddress;

        private Action<Stream, EndPoint, Action<byte[]>> _IncomeDataHandler;
        private NetworkStream _NetworkStream = null;

        public NetListener(INetServerEndpointSettings settings, Action<Stream, EndPoint, Action<byte[]>> incomeDataHandler)
        {
            this._serverAddress = settings;
            this._IncomeDataHandler = incomeDataHandler;
        }

        public void Start()
        {
            Debug.WriteLine($"Listener starts at {DateTime.Now.ToString()}. {this._serverAddress.ServerIpAddress}:{this._serverAddress.ServerPortNumber}");
            _Listener = new TcpListener(this._serverAddress.ServerIpAddress, this._serverAddress.ServerPortNumber);
            _Listener.Start();

            Task receive = Task.Run(() => LoopReceive());
        }

        public void Stop()
        {
            this.keepAlive = false;
            this._Listener.Stop();
        }

        private void LoopReceive()
        {
            try
            {
                Debug.WriteLine("Listen...");
                while (keepAlive)
                {
                    using (TcpClient tcpClient = _Listener.AcceptTcpClient()) // Waiting for a client
                    {
                        Debug.WriteLine($"Client connected from {tcpClient.Client.RemoteEndPoint}");
                      
                        using (_NetworkStream = tcpClient.GetStream())
                        {
                            this._IncomeDataHandler?.Invoke(_NetworkStream
                                , tcpClient.Client.RemoteEndPoint
                                , (data) =>
                                    {
                                        tcpClient.Client.Send(data);
                                    });
                        }

                      ////  newChannel.ConnectionClosed += (sender, channelUid) => { Debug.WriteLine($"Channel '{channelUid}' is closed."); this._Channels.RemoveAll(c => c.Uid == channelUid); };
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
                if (_Listener != null) _Listener.Stop();
            }
        }
    }
}
