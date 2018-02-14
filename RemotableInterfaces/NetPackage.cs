using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableInterfaces
{
    public class NetPackage
    {
        public byte[] Data { get; private set; }

        public static NetPackage Create(byte[] data) =>
            new NetPackage { Data = data };
    }
}