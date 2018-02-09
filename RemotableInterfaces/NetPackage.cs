using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableInterfaces
{
    public class NetPackage
    {
        public string serviceUid;
        public byte[] Data { get; private set; }

        public static NetPackage Create(string serviceUid, byte[] data) =>
            new NetPackage { Data = data };
    }
}