using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NAPS2.Config;
using NAPS2.Scan.Images.Transforms;

namespace NAPS2.Recovery
{
    public class RecoveryIndexManager : ConfigManager<RecoveryIndex>
    {
        private const string INDEX_FILE_NAME = "index.xml";

        public RecoveryIndexManager(DirectoryInfo recoveryFolder)
            : base(INDEX_FILE_NAME, recoveryFolder.FullName, null, () => new RecoveryIndex { Version = RecoveryIndex.CURRENT_VERSION })
        {
        }

        public RecoveryIndex Index => Config;

        protected override RecoveryIndex Deserialize(Stream configFileStream)
        {
            var serializer = new XmlSerializer(typeof(RecoveryIndex));
            return (RecoveryIndex)serializer.Deserialize(configFileStream);
        }
    }
}
