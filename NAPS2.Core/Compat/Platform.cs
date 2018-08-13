using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Compat
{
    public class Platform
    {
        private static IPlatformCompat _compat;

        static Platform()
        {
            if (Type.GetType("Mono.Runtime") != null)
            {
                _compat = new MonoPlatformCompat();
            }
            else
            {
                _compat = new DefaultPlatformCompat();
            }
        }

        public static IPlatformCompat Compat
        {
            get => _compat;
            set => _compat = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
