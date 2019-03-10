using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.ImportExport.Images
{
    public class ImageSettingsContainer : ImageSettingsProvider
    {
        public override ImageSettings ImageSettings => LocalImageSettings ?? UserConfig.Current.ImageSettings ?? new ImageSettings();

        public ImageSettings LocalImageSettings { get; set; }
    }
}
