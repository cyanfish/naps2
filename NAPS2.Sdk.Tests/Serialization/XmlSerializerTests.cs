using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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

        [Fact]
        public void SerializeNested()
        {
            var obj = new NestedPoco
            {
                Child = new Poco
                {
                    Str = "Test"
                }
            };
            var serializer = new XmlSerializer<NestedPoco>();
            var doc = serializer.SerializeToXDocument(obj);
            Assert.NotNull(doc.Root);
            var childEl = doc.Root.Element("Child");
            Assert.NotNull(childEl);
            var strEl = childEl.Element("Str");
            Assert.NotNull(strEl);
            Assert.Equal("Test", strEl.Value);

            var obj2 = serializer.DeserializeFromXDocument(doc);
            Assert.Equal("Test", obj2?.Child.Str);
        }

        [Fact]
        public void SerializeNull()
        {
            var xsi = (XNamespace)"http://www.w3.org/2001/XMLSchema-instance";
            var obj = new Poco { Str = null };
            var serializer = new XmlSerializer<Poco>();
            var doc = serializer.SerializeToXDocument(obj);
            Assert.NotNull(doc.Root);
            var xsiAttr = doc.Root.Attribute(XNamespace.Xmlns + "xsi");
            Assert.NotNull(xsiAttr);
            Assert.Equal(xsiAttr.Value, xsi.NamespaceName);
            var childEl = doc.Root.Element("Str");
            Assert.NotNull(childEl);
            Assert.True(childEl.IsEmpty);
            var nullAttr = childEl.Attribute(xsi + "nil");
            Assert.NotNull(nullAttr);
            Assert.Equal("true", nullAttr.Value);

            var obj2 = serializer.DeserializeFromXDocument(doc);
            Assert.Null(obj2.Str);
        }

        // TODO: Collections (root + nested)
        // TODO: Subtypes (with xsi:type)
        // TODO: Custom serialization
        // TODO: Ordering

        private class NestedPoco
        {
            public Poco Child { get; set; }
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
 