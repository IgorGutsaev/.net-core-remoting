using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableInterfaces
{
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class SomeClassB
    {
        [ProtoMember(1)]
        public string Uid { get; set; } = "Uid-1";
        [ProtoMember(2)]
        public int Value { get; set; } = 1;
    }

    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class SomeClassA
    {
        [ProtoMember(1)]
        public string Uid { get; set; } = "Uid-1";
        [ProtoMember(2)]
        public int Value { get; set; } = 1;
        [ProtoMember(3)]
        public DateTime Date { get; set; } = DateTime.Now;

        [ProtoMember(4)]
        public SomeClassB Child { get; set; }

        public SomeClassA()
        {
            this.Child = new SomeClassB { Uid = "Uid-2", Value = 2 };
        }
    }
}
