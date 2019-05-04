using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NAPS2.Serialization
{
    public abstract class CustomXmlSerializer
    {
        public abstract void SerializeObject(object obj, XElement element);

        public abstract object DeserializeObject(XElement element);
    }

    public abstract class CustomXmlSerializer<T> : CustomXmlSerializer
    {
        public override void SerializeObject(object obj, XElement element)
        {
            Serialize((T)obj, element);
        }

        protected abstract void Serialize(T obj, XElement element);

        public override object DeserializeObject(XElement element)
        {
            return Deserialize(element);
        }

        protected abstract T Deserialize(XElement element);
    }
}