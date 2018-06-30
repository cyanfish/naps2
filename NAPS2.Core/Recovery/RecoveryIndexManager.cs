﻿using NAPS2.Config;
using NAPS2.Scan.Images.Transforms;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

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
            var recoveryIndex = (RecoveryIndex)serializer.Deserialize(configFileStream);
            // Upgrade from V1 to V2
            if (recoveryIndex.Version == 1)
            {
                foreach (var img in recoveryIndex.Images)
                {
                    if (img.Transform != RotateFlipType.RotateNoneFlipNone)
                    {
                        img.TransformList.Add(new RotationTransform(img.Transform));
                        img.Transform = RotateFlipType.RotateNoneFlipNone;
                    }
                }
                recoveryIndex.Version = RecoveryIndex.CURRENT_VERSION;
            }
            return recoveryIndex;
        }
    }
}