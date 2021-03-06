﻿using System;

namespace RemotableInterfaces
{
    public interface IMyService
    {
        event EventHandler<Part> OnSomeBDetect;

        Unit Do(int valueInt, string valueString, Unit someA);

        int Do(string valueString, int valueInt, Unit someB);

        void CheckUnitNotNull(Unit someUnit);
    }
}
