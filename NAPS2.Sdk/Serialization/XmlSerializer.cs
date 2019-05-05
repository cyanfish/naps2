using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NAPS2.Util;

namespace NAPS2.Serialization
{
    public class XmlSerializer<T>
    {
        private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";

        private static readonly Dictionary<Type, XmlTypeInfo> TypeInfoCache = new Dictionary<Type, XmlTypeInfo>
        {
            { typeof(char), new XmlTypeInfo { CustomSerializer = new CharSerializer() } },
            { typeof(string), new XmlTypeInfo { CustomSerializer = new StringSerializer() } },
            { typeof(bool), new XmlTypeInfo { CustomSerializer = new BooleanSerializer() } },
            { typeof(byte), new XmlTypeInfo { CustomSerializer = new ByteSerializer() } },
            { typeof(sbyte), new XmlTypeInfo { CustomSerializer = new SByteSerializer() } },
            { typeof(short), new XmlTypeInfo { CustomSerializer = new Int16Serializer() } },
            { typeof(ushort), new XmlTypeInfo { CustomSerializer = new UInt16Serializer() } },
            { typeof(int), new XmlTypeInfo { CustomSerializer = new Int32Serializer() } },
            { typeof(uint), new XmlTypeInfo { CustomSerializer = new UInt32Serializer() } },
            { typeof(long), new XmlTypeInfo { CustomSerializer = new Int64Serializer() } },
            { typeof(ulong), new XmlTypeInfo { CustomSerializer = new UInt64Serializer() } },
            { typeof(float), new XmlTypeInfo { CustomSerializer = new SingleSerializer() } },
            { typeof(double), new XmlTypeInfo { CustomSerializer = new DoubleSerializer() } },
            { typeof(IntPtr), new XmlTypeInfo { CustomSerializer = new IntPtrSerializer() } },
            { typeof(UIntPtr), new XmlTypeInfo { CustomSerializer = new UIntPtrSerializer() } },
            { typeof(List<>), new XmlTypeInfo { CustomSerializer = new ListSerializer() } },
        };

        public void Serialize(T obj, Stream stream)
        {
            SerializeToXDocument(obj).Save(stream);
        }

        public XDocument SerializeToXDocument(T obj)
        {
            var doc = new XDocument();
            var root = SerializeToXElement(obj, GetElementNameForType(typeof(T)));
            root.SetAttributeValue(XNamespace.Xmlns + "xsi", Xsi.NamespaceName);
            doc.Add(root);
            return doc;
        }

        public XElement SerializeToXElement(T obj, string elementName)
        {
            return SerializeInternal(obj, elementName, typeof(T));
        }

        private static XElement SerializeInternal(object obj, string elementName, Type type)
        {
            var element = new XElement(elementName);
            if (obj == null)
            {
                element.SetAttributeValue(Xsi + "nil", "true");
                return element;
            }

            var actualType = obj.GetType();
            if (actualType != type)
            {
                element.SetAttributeValue(Xsi + "type", actualType.Name);
            }
            var typeInfo = GetTypeInfo(actualType);

            if (typeInfo.CustomSerializer != null)
            {
                typeInfo.CustomSerializer.SerializeObject(obj, element, type);
            }
            else
            {
                foreach (var propInfo in typeInfo.Properties)
                {
                    var child = SerializeInternal(propInfo.Property.GetValue(obj), propInfo.Property.Name, propInfo.Property.PropertyType);
                    element.Add(child);
                }
            }

            return element;
        }

        public T Deserialize(Stream stream)
        {
            return DeserializeFromXDocument(XDocument.Load(stream));
        }

        public T DeserializeFromXDocument(XDocument doc)
        {
            return DeserializeFromXElement(doc.Root);
        }

        public T DeserializeFromXElement(XElement element)
        {
            return (T)DeserializeInternal(element, typeof(T));
        }

        private static object DeserializeInternal(XElement element, Type type)
        {
            if (element.Attribute(Xsi + "nil")?.Value == "true")
            {
                return null;
            }

            var actualTypeName = element.Attribute(Xsi + "type")?.Value;
            var actualType = type;
            if (actualTypeName != null)
            {
                actualType = FindType(type, actualTypeName);
                if (actualType == null)
                {
                    throw new InvalidOperationException($"Could not find type {actualTypeName} with base type {type.FullName}");
                }
            }
            var typeInfo = GetTypeInfo(actualType);
            if (typeInfo.CustomSerializer != null)
            {
                return typeInfo.CustomSerializer.DeserializeObject(element, actualType);
            }
            var obj = Activator.CreateInstance(actualType);
            foreach (var propInfo in typeInfo.Properties)
            {
                // TODO: Detect unmapped elements
                var child = element.Element(propInfo.Property.Name);
                if (child != null)
                {
                    var childObj = DeserializeInternal(child, propInfo.Property.PropertyType);
                    propInfo.Property.SetValue(obj, childObj);
                }
            }
            return obj;
        }

        private static Type FindType(Type baseType, string actualTypeName)
        {
            lock (TypeInfoCache)
            {
                // TODO: This should not use the cache (which is nondeterministic), instead TypeInfo should store a set of known types (by walking the property tree and unioning)
                // TODO: Also have a separate set/logic for primitive and other built-in types
                return TypeInfoCache.Keys.FirstOrDefault(x => x.Name == actualTypeName && baseType.IsAssignableFrom(x));
            }
        }

        private static string GetElementNameForType(Type type)
        {
            if (type.IsArray)
            {
                return "ArrayOf" + GetElementNameForType(type.GetElementType());
            }
            if (type.IsGenericType)
            {
                string baseName;
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    baseName = "Array";
                }
                else
                {
                    var backtickIndex = type.Name.IndexOf('`');
                    baseName = type.Name.Substring(0, backtickIndex);
                }
                return baseName + "Of" + string.Join("", type.GetGenericArguments().Select(GetElementNameForType));
            }
            return type.Name;
        }

        private static XmlTypeInfo GetTypeInfo(Type type)
        {
            lock (TypeInfoCache)
            {
                return TypeInfoCache.GetOrSet(type, () =>
                {
                    var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var typeInfo = new XmlTypeInfo
                    {
                        Properties = props.Select(x => new XmlPropertyInfo
                        {
                            Property = x
                        }).ToArray()
                    };

                    if (type.IsGenericType)
                    {
                        var baseType = type.GetGenericTypeDefinition();
                        typeInfo.CustomSerializer = TypeInfoCache.Get(baseType)?.CustomSerializer;
                    }

                    if (type.IsArray)
                    {
                        typeInfo.CustomSerializer = new ArraySerializer();
                    }
                    else
                    {
                        // Verify we can create an instance to fail fast
                        Activator.CreateInstance(type);
                    }
                    return typeInfo;
                });
            }
        }

        private class XmlTypeInfo
        {
            public XmlPropertyInfo[] Properties { get; set; }

            public CustomXmlSerializer CustomSerializer { get; set; }
        }

        private class XmlPropertyInfo
        {
            public PropertyInfo Property { get; set; }
        }

        private class CharSerializer : CustomXmlSerializer<char>
        {
            protected override void Serialize(char obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override char Deserialize(XElement element)
            {
                return char.Parse(element.Value);
            }
        }

        private class StringSerializer : CustomXmlSerializer<string>
        {
            protected override void Serialize(string obj, XElement element)
            {
                element.Value = obj;
            }

            protected override string Deserialize(XElement element)
            {
                return element.Value;
            }
        }

        private class BooleanSerializer : CustomXmlSerializer<bool>
        {
            protected override void Serialize(bool obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override bool Deserialize(XElement element)
            {
                return bool.Parse(element.Value);
            }
        }

        private class ByteSerializer : CustomXmlSerializer<byte>
        {
            protected override void Serialize(byte obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override byte Deserialize(XElement element)
            {
                return byte.Parse(element.Value);
            }
        }

        private class SByteSerializer : CustomXmlSerializer<sbyte>
        {
            protected override void Serialize(sbyte obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override sbyte Deserialize(XElement element)
            {
                return sbyte.Parse(element.Value);
            }
        }

        private class Int16Serializer : CustomXmlSerializer<short>
        {
            protected override void Serialize(short obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override short Deserialize(XElement element)
            {
                return short.Parse(element.Value);
            }
        }

        private class UInt16Serializer : CustomXmlSerializer<ushort>
        {
            protected override void Serialize(ushort obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override ushort Deserialize(XElement element)
            {
                return ushort.Parse(element.Value);
            }
        }

        private class Int32Serializer : CustomXmlSerializer<int>
        {
            protected override void Serialize(int obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override int Deserialize(XElement element)
            {
                return int.Parse(element.Value);
            }
        }

        private class UInt32Serializer : CustomXmlSerializer<uint>
        {
            protected override void Serialize(uint obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override uint Deserialize(XElement element)
            {
                return uint.Parse(element.Value);
            }
        }

        private class Int64Serializer : CustomXmlSerializer<long>
        {
            protected override void Serialize(long obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override long Deserialize(XElement element)
            {
                return long.Parse(element.Value);
            }
        }

        private class UInt64Serializer : CustomXmlSerializer<ulong>
        {
            protected override void Serialize(ulong obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override ulong Deserialize(XElement element)
            {
                return ulong.Parse(element.Value);
            }
        }

        private class SingleSerializer : CustomXmlSerializer<float>
        {
            protected override void Serialize(float obj, XElement element)
            {
                element.Value = obj.ToString(CultureInfo.InvariantCulture);
            }

            protected override float Deserialize(XElement element)
            {
                return float.Parse(element.Value);
            }
        }

        private class DoubleSerializer : CustomXmlSerializer<double>
        {
            protected override void Serialize(double obj, XElement element)
            {
                element.Value = obj.ToString(CultureInfo.InvariantCulture);
            }

            protected override double Deserialize(XElement element)
            {
                return double.Parse(element.Value);
            }
        }

        private class IntPtrSerializer : CustomXmlSerializer<IntPtr>
        {
            protected override void Serialize(IntPtr obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override IntPtr Deserialize(XElement element)
            {
                return (IntPtr)long.Parse(element.Value);
            }
        }

        private class UIntPtrSerializer : CustomXmlSerializer<UIntPtr>
        {
            protected override void Serialize(UIntPtr obj, XElement element)
            {
                element.Value = obj.ToString();
            }

            protected override UIntPtr Deserialize(XElement element)
            {
                return (UIntPtr)ulong.Parse(element.Value);
            }
        }

        private class ArraySerializer : CustomXmlSerializer
        {
            public override void SerializeObject(object obj, XElement element, Type type)
            {
                var itemType = type.GetElementType() ?? throw new ArgumentException("Not an array type");
                var list = (IList)obj;
                foreach (var item in list)
                {
                    element.Add(SerializeInternal(item, itemType.Name, itemType));
                }
            }

            public override object DeserializeObject(XElement element, Type type)
            {
                var itemType = type.GetElementType() ?? throw new ArgumentException("Not an array type");
                var elements = element.Elements().ToArray();
                var array = Array.CreateInstance(itemType, elements.Length);
                for (int i = 0; i < elements.Length; i++)
                {
                    array.SetValue(DeserializeInternal(elements[i], itemType), i);
                }
                return array;
            }
        }

        private class ListSerializer : CustomXmlSerializer
        {
            public override void SerializeObject(object obj, XElement element, Type type)
            {
                var itemType = GetItemType(type);
                var list = (IList)obj;
                foreach (var item in list)
                {
                    element.Add(SerializeInternal(item, itemType.Name, itemType));
                }
            }

            private Type GetItemType(Type type)
            {
                if (type.IsGenericType)
                {
                    var typeArgs = type.GetGenericArguments();
                    if (typeArgs.Length != 1)
                    {
                        throw new ArgumentException("Unexpected number of generic arguments for list type");
                    }
                    return typeArgs[0];
                }
                return typeof(object);
            }

            public override object DeserializeObject(XElement element, Type type)
            {
                var itemType = GetItemType(type);
                var list = (IList)Activator.CreateInstance(type);
                foreach (var itemElement in element.Elements())
                {
                    list.Add(DeserializeInternal(itemElement, itemType));
                }
                return list;
            }
        }
    }
}
