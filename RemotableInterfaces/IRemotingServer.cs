using System;

namespace RemotableInterfaces
{
    public interface IRemotingServer: IDisposable
    {
        void Start();
        bool IsEnable();
    }
}