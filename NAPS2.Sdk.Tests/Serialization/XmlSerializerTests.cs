using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Sdk.Tests.Serialization
{
    public class XmlSerializerTests
    {
        [Fact]
        public void SerializePoco()
        {
            var obj = new Poco { Str = "Hello world", Int = 42 };
            var serializer = new XmlSerializer<Poco>();
            var doc = serializer.SerializeToXDocument(obj);
            Assert.NotNull(doc.Root);
            Assert.Equal("Poco", doc.Root.Name);
            Assert.Equal(2, doc.Root.Elements().Count());
            var strEl = doc.Root.Element("Str");
            Assert.NotNull(strEl);
            var intEl = doc.Root.Element("Int");
            Assert.NotNull(intEl);
            Assert.Equal("Hello world", strEl.Value);
            Assert.Equal("42", intEl.Value);

            var obj2 = serializer.DeserializeFromXDocument(doc);
            Assert.Equal("Hello world", obj2.Str);
            Assert.Equal(42, obj2.Int);
        }

        [Fact]
        public void SerializePrivateSetter()
        {
            var obj = new PrivateSetter("Hello");
            var serializer = new XmlSerializer<PrivateSetter>();
            var doc = serializer.SerializeToXDocument(obj);
            Assert.NotNull(doc.Root);
            Assert.Equal("PrivateSetter", doc.Root.Name);
            Assert.Single(doc.Root.Elements());
            var strEl = doc.Root.Element("Str");
            Assert.NotNull(strEl);
            Assert.Equal("Hello", strEl.Value);

            var obj2 = serializer.DeserializeFromXDocument(doc);
            Assert.Equal("Hello", obj2.Str);
        }

        private class Poco
        {
            public string Str { get; set; }

            public int Int { get; set; }
        }

        private class PrivateSetter
        {
            public PrivateSetter()
            {
            }

            public PrivateSetter(string str)
            {
                Str = str;
            }

            public string Str { get; private set; }
        }
    }
}
