namespace RemotableInterfaces
{
    public interface IRemotingServer
    {
        void Start();
        void Stop();
        bool IsEnable();
    }
}
