using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableObjects
{
    public class MyService : IMyService
    {
        public event EventHandler<SomeClassB> OnSomeBDetect;

        public MyService()
        {

        }

        public SomeClassA Do(int valueInt, string valueString, SomeClassA someA)
        {
            this.OnSomeBDetect?.Invoke(null, someA.Child);
            return someA;// $"Hello, world! {valueInt} {valueString}";
        }

        public int Do(string valueString, int valueInt, SomeClassA someA)
        {
            return valueInt;
        }
    }
}
