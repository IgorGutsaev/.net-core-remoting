using RemotableInterfaces;
using System;
using System.Collections.Generic;

namespace RemotableTests
{
    public class TestDataGenerator
    {
        static public IEnumerable<object[]> GetUnitFromDataGenerator()
        {
            yield return new object[]
            {
                1,
                "value",
                new Unit { Uid = "1" }
            };

            yield return new object[]
            {
                0,
                "",
                new Unit { Value = 1, Date = DateTime.Now }

            };

            yield return new object[]
            {
                int.MaxValue,
                "",
                new Unit { Child = new Part { Uid = "2", Value= 5 } }
            };

            yield return new object[]
            {
                0,
                "someData ",
                new Unit { Date = DateTime.Now }
            };
        }
    }
}