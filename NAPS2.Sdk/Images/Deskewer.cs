using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images.Storage;
using NAPS2.Util;

namespace NAPS2.Images
{
    public abstract class Deskewer
    {
        private static Deskewer _default = new HoughLineDeskewer();

        public static Deskewer Default
        {
            get
            {
                TestingContext.NoStaticDefaults();
                return _default;
            }
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public abstract double GetSkewAngle(IImage image);
    }
}
