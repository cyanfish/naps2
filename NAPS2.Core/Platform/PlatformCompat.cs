using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Platform
{
    public class PlatformCompat
    {
        private static IRuntimeCompat _runtimeCompat;
        private static ISystemCompat _systemCompat;

        static PlatformCompat()
        {
            if (Type.GetType("Mono.Runtime") != null)
            {
                _runtimeCompat = new MonoRuntimeCompat();
            }
            else
            {
                _runtimeCompat = new DefaultRuntimeCompat();
            }

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                _systemCompat = new WindowsSystemCompat();
            }
            else
            {
                _systemCompat = new LinuxSystemCompat();
            }
        }

        public static IRuntimeCompat Runtime
        {
            get => _runtimeCompat;
            set => _runtimeCompat = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static ISystemCompat System
        {
            get => _systemCompat;
            set => _systemCompat = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
