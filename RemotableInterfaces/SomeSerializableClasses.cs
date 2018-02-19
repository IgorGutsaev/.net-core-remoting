using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemotableInterfaces
{
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class Part
    {
        [ProtoMember(1)]
        public string Uid { get; set; } = "Uid-1";
        [ProtoMember(2)]
        public int Value { get; set; } = 1;

        public override string ToString()
        {
            return $"{this.Uid}: {this.Value}";
        }
    }

    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class Unit
    {
        [ProtoMember(1)]
        public string Uid { get; set; } = "Uid-1";
        [ProtoMember(2)]
        public int Value { get; set; } = 1;
        [ProtoMember(3)]
        public DateTime Date { get; set; } = DateTime.Now;

        [ProtoMember(4)]
        public Part Child { get; set; }

        public Unit()
        {
            this.Child = new Part { Uid = "Uid-2", Value = 2 };
        }

        public override string ToString()
        {
            return $"{this.Uid}: {this.Value}/{this.Date.ToString("G")}. [{this.Child}]";
        }
    }
}
