using RemotableInterfaces;
using System;
using System.Reflection;

namespace RemotableClient
{
    internal class MyServiceProxy : BaseServiceProxy, IMyService
    {
        public event EventHandler<SomeClassB> OnSomeBDetect;

        public MyServiceProxy(IClientProxy clientProxy)
            : base(clientProxy)
        {

        }

        public SomeClassA Do(int valueInt, string valueString, SomeClassA someA)
        {
            MethodBase mBase = System.Reflection.MethodBase.GetCurrentMethod();
            var parameters = new object[] { valueInt, valueString, someA };
            MethodParameter[] _params = PrepareParameters(mBase, parameters);

            return this.InvokeMethod<SomeClassA>(mBase.Name, _params);
        }

        public int Do(string valueString, int valueInt, SomeClassA someA)
        {
            MethodBase mBase = System.Reflection.MethodBase.GetCurrentMethod();
            var parameters = new object[] { valueString, valueInt,  someA };
            MethodParameter[] packedParameters = PrepareParameters(mBase, parameters);

            return this.InvokeMethod<int>(mBase.Name, packedParameters);
        }
    }
}