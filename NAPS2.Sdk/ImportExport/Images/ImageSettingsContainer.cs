using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.ImportExport.Images
{
    public class ImageSettingsContainer
    {
        private ImageSettings localImageSettings;

        public ImageSettings ImageSettings
        {
            get => localImageSettings ?? UserConfig.Current.ImageSettings ?? new ImageSettings();
            set => localImageSettings = value;
        }
    }
}
