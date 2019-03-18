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
            Order = line;
        }
    }
}
