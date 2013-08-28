using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.Config;
using Ninject;
using NLog;

namespace NAPS2.Scan
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
