using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.ImportExport.Images
{
    public class ImageSettingsContainer
    {
        private readonly UserConfigManager userConfigManager;

        private ImageSettings localImageSettings;

        public ImageSettingsContainer(UserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        public ImageSettings ImageSettings
        {
            get { return localImageSettings ?? userConfigManager.Config.ImageSettings ?? new ImageSettings(); }
            set { localImageSettings = value; }
        }
    }
}
