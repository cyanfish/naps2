using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NAPS2.Config.Experimental
{
    public abstract class ConfigPropAttribute : XmlElementAttribute
    {
        protected ConfigPropAttribute(int line)
        {
            // Ideally we would use XmlElementAttribute.Order so the generated XML files have a nice ordering.
            // However, that can break deserialization in case the order changes (e.g. new properties).
            // So we have to be satisfied with this, which can at least be used to generate appsettings.xml.
            ConfigOrdering = line;
            // Setting nullable ensures all properties are included in the output. Otherwise there is an
            // inconsistency between reference types and nullable value types.
            IsNullable = true;
        }

        public int ConfigOrdering { get; set; }
    }
}
