using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.ImportExport.Images
{
    public abstract class ImageSettingsProvider
    {
        private static ImageSettingsProvider _default = Wrap(new ImageSettings());

        public static ImageSettingsProvider Wrap(ImageSettings imageSettings) => new Wrapper(imageSettings);

        public static ImageSettingsProvider Default
        {
            get
            {
                TestingContext.NoStaticDefaults();
                return _default;
            }
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public abstract ImageSettings ImageSettings { get; }

        private class Wrapper : ImageSettingsProvider
        {
            public Wrapper(ImageSettings imageSettings)
            {
                ImageSettings = imageSettings;
            }

            public override ImageSettings ImageSettings { get; }
        }
    }
}
