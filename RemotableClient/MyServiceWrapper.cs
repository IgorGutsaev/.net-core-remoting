using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProtoBuf;

namespace RemotableClient
{
    internal class MyServiceWrapper : BaseServiceWrapper, IMyService
    {
        public event EventHandler<SomeClassB> OnSomeBDetect;

        public MyServiceWrapper(IClientProxy clientProxy)
            : base(clientProxy)
        {

        }

        public SomeClassA Do(int valueInt, string valueString, SomeClassA someA)
        {
            MethodBase mBase = System.Reflection.MethodBase.GetCurrentMethod();
            var parameters = new object[] { valueInt, valueString, someA };
            MethodParameterMsg[] packedParameters = PrepareParameters(mBase, parameters);

            return this.InvokeMethod<SomeClassA>(mBase.Name, packedParameters);
        }

        public int Do(string valueString, int valueInt, SomeClassA someA)
        {
            MethodBase mBase = System.Reflection.MethodBase.GetCurrentMethod();
            var parameters = new object[] { valueString, valueInt,  someA };
            MethodParameterMsg[] packedParameters = PrepareParameters(mBase, parameters);

            return this.InvokeMethod<int>(mBase.Name, packedParameters);
        }

        private MethodParameterMsg[] PrepareParameters(MethodBase mBase, dynamic data)
        {
            List<MethodParameterMsg> result = new List<MethodParameterMsg>();

            int index = 0;
            foreach (var p in mBase.GetParameters())
            {
                var t = data[index];
                index++;

                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, t);
                    stream.Position = 0;

                    MethodParameterMsg param = new MethodParameterMsg() { Name = p.Name, Type = (t.GetType().IsGenericType ? t.GetType().FullName : t.GetType().AssemblyQualifiedName), Value = Google.Protobuf.ByteString.FromStream(stream) };
                    result.Add(param);
                }
            }

            return result.ToArray();
        }
    }
}