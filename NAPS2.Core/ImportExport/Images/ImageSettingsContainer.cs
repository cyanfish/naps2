using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.ImportExport.Images
{
    public class ImageSettingsContainer
    {
        private readonly IUserConfigManager userConfigManager;

        private ImageSettings localImageSettings;

        public ImageSettingsContainer(IUserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        public ImageSettings ImageSettings
        {
            get => localImageSettings ?? userConfigManager.Config.ImageSettings ?? new ImageSettings();
            set => localImageSettings = value;
        }
    }
}
