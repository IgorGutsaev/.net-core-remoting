﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using RemotableServer;
using RemotableInterfaces;
using System.IO;

namespace RemotableInterface
{
    /// <summary>
    /// 
    /// </summary>
    public interface IClientProxy
    {
        object Invoke(object data); 
    }
}
