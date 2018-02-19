using RemotableInterfaces;
using System;

namespace RemotableObjects
{
    public class MyService : IMyService
    {
        public event EventHandler<Part> OnSomeBDetect;

        public Unit Do(int valueInt, string valueString, Unit someUnit)
        {
            this.OnSomeBDetect?.Invoke(null, someUnit.Child);
            return someUnit;// $"Hello, world! {valueInt} {valueString}";
        }

        public int Do(string valueString, int valueInt, Unit someUnit)
        {
            return valueInt;
        }

        public void CheckUnitNotNull(Unit someUnit)
        {
            throw new ArgumentNullException("Argument must declared!");
        }
    }
}
