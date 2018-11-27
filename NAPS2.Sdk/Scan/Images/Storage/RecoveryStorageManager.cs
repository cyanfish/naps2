using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
using NAPS2.Recovery;

namespace NAPS2.Scan.Images.Storage
{
    public class RecoveryStorageManager : FileStorageManager
    {
        private ConfigManager<RecoveryIndex> indexConfigManager;

        public RecoveryStorageManager(string recoveryFolderPath)
        {
            indexConfigManager = new ConfigManager<RecoveryIndex>("index.xml", recoveryFolderPath, null, RecoveryIndex.Create);
        }

        public override void Detach(string path)
        {
            //lock (this)
            //{
            //    indexConfigManager.Config.Images.Remove(IndexImage);
            //    indexConfigManager.Save();
            //}
        }
    }
}
