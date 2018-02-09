using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableInterfaces
{
    // Moved to protobuf
    //public enum RemotingCommands
    //{
    //    Unknown = 0,
    //    ConnectionRequest = 1,
    //    ConnectionResponse = 2,
    //    HandshakeRequest = 3,
    //    HandshakeResponse = 4,
    //    QueryInterface = 5,
    //    ReleaseInterface = 6,
    //    InvokeMethod = 7,
    //    TriggerEvent = 8,
    //}

    //public abstract class ProtoType
    //{
    //    public readonly RemotingCommands objectId;
    //    public readonly byte[] objectIdAsBytes;

    //    public ProtoType()
    //    {
    //        Type t = this.GetType();
    //        if (t == typeof(MyServiceMessageTypeA)) objectId = RemotingCommands.QueryInterface; // to identify the object before deserialization
    //        // else if (t == typeof(MyServiceMessageTypeB)) objectId = RemotingCommands.InvokeMethod; // to identify the object before deserialization
    //        else throw new Exception("object type unknown");
    //        objectIdAsBytes = BitConverter.GetBytes((Int16)objectId);
    //    } // constructor
    //} // class 

}
