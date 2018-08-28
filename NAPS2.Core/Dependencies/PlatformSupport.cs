using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Dependencies
{
    public class PlatformSupport
    {
        public static readonly PlatformSupport Windows = new PlatformSupport(() => Environment.OSVersion.Platform == PlatformID.Win32NT);

        public static readonly PlatformSupport WindowsXp = Windows.And(new PlatformSupport(() => Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1));

        public static readonly PlatformSupport ModernWindows = Windows.Except(WindowsXp);

        public static readonly PlatformSupport X64 = new PlatformSupport(() => Environment.Is64BitOperatingSystem);

        public static readonly PlatformSupport ModernWindows64 = ModernWindows.And(X64);

        public static readonly PlatformSupport Linux = new PlatformSupport(() => Environment.OSVersion.Platform == PlatformID.Unix);

        private readonly Func<bool> predicate;

        private PlatformSupport(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public bool Validate() => predicate();

        public PlatformSupport And(params PlatformSupport[] platforms)
        {
            return new PlatformSupport(() => Validate() && platforms.All(x => x.Validate()));
        }

        public PlatformSupport Or(params PlatformSupport[] platforms)
        {
            return new PlatformSupport(() => Validate() || platforms.Any(x => x.Validate()));
        }

        public PlatformSupport Except(params PlatformSupport[] platforms)
        {
            return new PlatformSupport(() => Validate() && platforms.All(x => !x.Validate()));
        }
    }
}
