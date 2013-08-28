using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Scan.Recovery
{
    public class RecoveryIndexManager : ConfigManager<RecoveryIndex>
    {
        private const string INDEX_FILE_NAME = "index.xml";

        public RecoveryIndexManager(DirectoryInfo recoveryFolder)
            : base(INDEX_FILE_NAME, recoveryFolder.FullName, null, () => new RecoveryIndex { Version = RecoveryIndex.CURRENT_VERSION })
        {
        }

        public RecoveryIndex Index
        {
            get
            {
                return Config;
            }
        }
    }
}
