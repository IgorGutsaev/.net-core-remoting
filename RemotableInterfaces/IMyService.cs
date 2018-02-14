using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableInterfaces
{
    public interface IMyService
    {
        event EventHandler<SomeClassB> OnSomeBDetect;

        SomeClassA Do(int valueInt, string valueString, SomeClassA someA);

        int Do(string valueString, int valueInt, SomeClassA someA);
    }
}
