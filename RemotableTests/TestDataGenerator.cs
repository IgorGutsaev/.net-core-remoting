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
                new Unit { Value = 1, Date = DateTime.Now },
                new Unit { Child = new Part { Uid = "2", Value= 4 }  },
                new Unit { Uid = "1" }
            };

            yield return new object[]
            {
                new Unit { Date = DateTime.Now },
                new Unit { Child = new Part()  },
                new Unit { Date = DateTime.Now }
            };
        }
    }
}