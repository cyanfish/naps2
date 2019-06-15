using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using NAPS2.Util;

namespace NAPS2.Serialization
{
    public abstract class XmlSerializer
    {
        protected static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";

        protected static Dictionary<Type, CustomXmlSerializer> CustomSerializerCache;

        protected static Dictionary<Type, List<CustomXmlTypes>> CustomTypesCache;

        protected static readonly Dictionary<Type, XmlTypeInfo> TypeInfoCache = new Dictionary<Type, XmlTypeInfo>
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
            { typeof(HashSet<>), new XmlTypeInfo { CustomSerializer = new CollectionSerializer() } },
            { typeof(DateTime), new XmlTypeInfo { CustomSerializer = new DateTimeSerializer() } },
            { typeof(Nullable<>), new XmlTypeInfo { CustomSerializer = new NullableSerializer() } },
        };

        protected static XElement SerializeInternal(object obj, XElement element, Type type)
        {
            if (obj == null)
            {
                element.SetAttributeValue(Xsi + "nil", "true");
                return element;
            }

            var actualType = Nullable.GetUnderlyingType(type) != null ? type : obj.GetType();
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
                    var child = SerializeInternal(propInfo.Property.GetValue(obj), new XElement(propInfo.Property.Name), propInfo.Property.PropertyType);
                    element.Add(child);
                }
            }

            return element;
        }

        protected static object DeserializeInternal(XElement element, Type type)
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
            var obj = Activator.CreateInstance(actualType, true);
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

        protected static Type FindType(Type baseType, string actualTypeName)
        {
            lock (TypeInfoCache)
            {
                return GetTypeInfo(baseType).KnownTypes?.SingleOrDefault(x => GetElementNameForType(x) == actualTypeName);
            }
        }

        protected static string GetElementNameForType(Type type)
        {
            // TODO: Cache this?
            var xmlNameAttribute = type.GetCustomAttributes(typeof(XmlTypeAttribute)).Cast<XmlTypeAttribute>().FirstOrDefault();
            if (xmlNameAttribute != null)
            {
                return xmlNameAttribute.TypeName;
            }
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

        protected static XmlTypeInfo GetTypeInfo(Type type)
        {
            lock (TypeInfoCache)
            {
                return TypeInfoCache.GetOrSet(type, () =>
                {
                    var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(x => x.GetMethod != null && x.SetMethod != null && !x.GetCustomAttributes(typeof(XmlIgnoreAttribute)).Any());
                    var typeInfo = new XmlTypeInfo
                    {
                        Properties = props.Select(x => new XmlPropertyInfo
                        {
                            Property = x
                        }).ToArray(),
                        CustomSerializer = GetCustomSerializer(type)
                    };

                    if (typeInfo.CustomSerializer == null)
                    {
                        if (type.IsGenericType)
                        {
                            var baseType = type.GetGenericTypeDefinition();
                            typeInfo.CustomSerializer = TypeInfoCache.Get(baseType)?.CustomSerializer;
                        }
                        if (type.IsArray)
                        {
                            typeInfo.CustomSerializer = new ArraySerializer();
                        }
                        else if (type.IsEnum)
                        {
                            typeInfo.CustomSerializer = new EnumSerializer();
                        }
                    }

                    if (typeInfo.CustomSerializer == null)
                    {
                        typeInfo.KnownTypes = props
                            .Select(x => GetTypeInfo(x.PropertyType).KnownTypes)
                            .Where(x => x != null)
                            .Append(GetKnownTypes(type))
                            .Append(type.GetCustomAttributes(typeof(XmlIncludeAttribute)).Cast<XmlIncludeAttribute>().Select(x => x.Type))
                            .Aggregate(new HashSet<Type>(), (x, y) => new HashSet<Type>(x.Union(y)));
                    }

                    return typeInfo;
                });
            }
        }

        protected static IEnumerable<Type> GetKnownTypes(Type type)
        {
            lock (TypeInfoCache)
            {
                if (CustomTypesCache == null)
                {
                    CustomTypesCache = new Dictionary<Type, List<CustomXmlTypes>>();
                    foreach (var customTypesType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
                    {
                        if (customTypesType.BaseType != null
                            && typeof(CustomXmlTypes).IsAssignableFrom(customTypesType)
                            && customTypesType.BaseType.IsGenericType
                            && typeof(CustomXmlTypes<>) == customTypesType.BaseType.GetGenericTypeDefinition())
                        {
                            var typeParam = customTypesType.BaseType.GetGenericArguments()[0];
                            CustomTypesCache.GetOrSet(typeParam, () => new List<CustomXmlTypes>()).Add((CustomXmlTypes)Activator.CreateInstance(customTypesType));
                        }
                    }
                }
                return CustomTypesCache.Get(type)?.SelectMany(x => x.GetKnownTypes(type)) ?? Enumerable.Empty<Type>();
            }
        }

        protected static CustomXmlSerializer GetCustomSerializer(Type type)
        {
            lock (TypeInfoCache)
            {
                if (CustomSerializerCache == null)
                {
                    CustomSerializerCache = new Dictionary<Type, CustomXmlSerializer>();
                    foreach (var customSerializerType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
                    {
                        if (customSerializerType.BaseType != null
                            && typeof(CustomXmlSerializer).IsAssignableFrom(customSerializerType)
                            && customSerializerType.BaseType.IsGenericType
                            && typeof(CustomXmlSerializer<>) == customSerializerType.BaseType.GetGenericTypeDefinition())
                        {
                            var typeParam = customSerializerType.BaseType.GetGenericArguments()[0];
                            CustomSerializerCache[typeParam] = (CustomXmlSerializer)Activator.CreateInstance(customSerializerType);
                        }
                    }
                }
                return CustomSerializerCache.Get(type);
            }
        }

        protected class XmlTypeInfo
        {
            public XmlPropertyInfo[] Properties { get; set; }

            public CustomXmlSerializer CustomSerializer { get; set; }

            public HashSet<Type> KnownTypes { get; set; }
        }

        protected class XmlPropertyInfo
        {
            public PropertyInfo Property { get; set; }
        }

        protected class CharSerializer : CustomXmlSerializer<char>
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

        protected class StringSerializer : CustomXmlSerializer<string>
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

        protected class BooleanSerializer : CustomXmlSerializer<bool>
        {
            protected override void Serialize(bool obj, XElement element)
            {
                element.Value = obj ? "true" : "false";
            }

            protected override bool Deserialize(XElement element)
            {
                return bool.Parse(element.Value);
            }
        }

        protected class ByteSerializer : CustomXmlSerializer<byte>
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

        protected class SByteSerializer : CustomXmlSerializer<sbyte>
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

        protected class Int16Serializer : CustomXmlSerializer<short>
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

        protected class UInt16Serializer : CustomXmlSerializer<ushort>
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

        protected class Int32Serializer : CustomXmlSerializer<int>
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

        protected class UInt32Serializer : CustomXmlSerializer<uint>
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

        protected class Int64Serializer : CustomXmlSerializer<long>
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

        protected class UInt64Serializer : CustomXmlSerializer<ulong>
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

        protected class SingleSerializer : CustomXmlSerializer<float>
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

        protected class DoubleSerializer : CustomXmlSerializer<double>
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

        protected class IntPtrSerializer : CustomXmlSerializer<IntPtr>
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

        protected class UIntPtrSerializer : CustomXmlSerializer<UIntPtr>
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

        protected class ArraySerializer : CustomXmlSerializer
        {
            public override void SerializeObject(object obj, XElement element, Type type)
            {
                var itemType = type.GetElementType() ?? throw new ArgumentException("Not an array type");
                var list = (IList)obj;
                foreach (var item in list)
                {
                    element.Add(SerializeInternal(item, new XElement(itemType.Name), itemType));
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

        protected class CollectionSerializer : CustomXmlSerializer
        {
            public override void SerializeObject(object obj, XElement element, Type type)
            {
                var itemType = GetItemType(type);
                var list = (IEnumerable)obj;
                foreach (var item in list)
                {
                    element.Add(SerializeInternal(item, new XElement(itemType.Name), itemType));
                }
            }

            protected Type GetItemType(Type type)
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
                var list = Activator.CreateInstance(type, true);
                var add = list.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance) ?? throw new ArgumentException("Collection type has no Add method");
                foreach (var itemElement in element.Elements())
                {
                    add.Invoke(list, new[] { DeserializeInternal(itemElement, itemType) });
                }
                return list;
            }
        }

        protected class ListSerializer : CollectionSerializer
        {
            public override object DeserializeObject(XElement element, Type type)
            {
                var itemType = GetItemType(type);
                var list = (IList)Activator.CreateInstance(type, true);
                foreach (var itemElement in element.Elements())
                {
                    list.Add(DeserializeInternal(itemElement, itemType));
                }
                return list;
            }
        }

        protected class DateTimeSerializer : CustomXmlSerializer<DateTime>
        {
            protected override void Serialize(DateTime obj, XElement element)
            {
                element.Value = obj.ToString("O", CultureInfo.InvariantCulture);
            }

            protected override DateTime Deserialize(XElement element)
            {
                return DateTime.Parse(element.Value, CultureInfo.InvariantCulture);
            }
        }

        protected class NullableSerializer : CustomXmlSerializer
        {
            public override void SerializeObject(object obj, XElement element, Type type)
            {
                SerializeInternal(obj, element, Nullable.GetUnderlyingType(type));
            }

            public override object DeserializeObject(XElement element, Type type)
            {
                return DeserializeInternal(element, Nullable.GetUnderlyingType(type));
            }
        }

        protected class EnumSerializer : CustomXmlSerializer
        {
            public override void SerializeObject(object obj, XElement element, Type type)
            {
                element.Value = obj.ToString();
            }

            public override object DeserializeObject(XElement element, Type type)
            {
                return Enum.Parse(type, element.Value);
            }
        }
    }

    public class XmlSerializer<T> : XmlSerializer, ISerializer<T>
    {
        public void Serialize(Stream stream, T obj)
        {
            using (var writer = new XmlTextWriter(new StreamWriter(stream, new UTF8Encoding(false), 1024, true)) { Formatting = Formatting.Indented })
            {
                SerializeToXDocument(obj).Save(writer);
                writer.Dispose();
            }
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
            return SerializeInternal(obj, new XElement(elementName), typeof(T));
        }

        public XElement SerializeToXElement(T obj, XElement element)
        {
            return SerializeInternal(obj, element, typeof(T));
        }

        public T Deserialize(Stream stream)
        {
            return DeserializeFromXDocument(XDocument.Load(stream));
        }

        public T DeserializeFromXDocument(XDocument doc)
        {
            if (doc.Root?.Name != GetElementNameForType(typeof(T)))
            {
                throw new InvalidOperationException($"Could not map XML element <{doc.Root?.Name}> to {typeof(T).FullName}. Expected <{GetElementNameForType(typeof(T))}>.");
            }
            return DeserializeFromXElement(doc.Root);
        }

        public T DeserializeFromXElement(XElement element)
        {
            return (T)DeserializeInternal(element, typeof(T));
        }
    }
}
