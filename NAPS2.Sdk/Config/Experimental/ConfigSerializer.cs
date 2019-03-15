using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Config.Experimental
{
    public class ConfigSerializer : ISerializer<CommonConfig>
    {
        public ConfigSerializer(ConfigReadMode mode)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Stream stream, CommonConfig obj)
        {
            throw new NotImplementedException();
        }

        public CommonConfig Deserialize(Stream stream) => throw new NotImplementedException();
    }
}