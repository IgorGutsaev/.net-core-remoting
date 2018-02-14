using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Xunit;
using Google.Protobuf;
using ProtoBuf;

namespace RemotableTests
{
    public class ByteTest
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
            public DateTime Date  { get; set; } = DateTime.Now;

            [ProtoMember(4)]
            public SomeClassB Child { get; set; }

            public SomeClassA()
            {
                this.Child = new SomeClassB { Uid = "Uid-2", Value = 2 };
            }

            public static byte[] ProtoSerialize<T>(T record) where T : class
            {
                if (null == record) return null;

                try
                {
                    using (var stream = new MemoryStream())
                    {
                        Serializer.Serialize(stream, record);
                        return stream.ToArray();
                    }
                }
                catch
                {
                    // Log error
                    throw;
                }
            }

            public static T ProtoDeserialize<T>(byte[] data) where T : class
            {
                if (null == data) return null;

                try
                {
                    using (var stream = new MemoryStream(data))
                    {
                        return Serializer.Deserialize<T>(stream);
                    }
                }
                catch
                {
                    // Log error
                    throw;
                }
            }
        }

        [Fact] 
        public void Test_Converter()
        {
            SomeClassA a = new SomeClassA();
            byte[] data = SomeClassA.ProtoSerialize<SomeClassA>(a);
            SomeClassA restore = SomeClassA.ProtoDeserialize<SomeClassA>(data);
        }
    }
}
