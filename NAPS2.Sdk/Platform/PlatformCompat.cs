namespace NAPS2.Platform;

public class PlatformCompat
{
    private static IRuntimeCompat _runtimeCompat;
    private static ISystemCompat _systemCompat;

    static PlatformCompat()
    {
        // TODO: Drop mono support
        if (Type.GetType("Mono.Runtime") != null)
        {
            _runtimeCompat = new MonoRuntimeCompat();
        }
        else
        {
            _runtimeCompat = new DefaultRuntimeCompat();
        }

#if NET6_0_OR_GREATER
        if (OperatingSystem.IsWindows()) {
            _systemCompat = Environment.Is64BitProcess ? new Windows64SystemCompat() : new Windows32SystemCompat();
        } else if (OperatingSystem.IsMacOS()) {
            _systemCompat = new MacSystemCompat();
        } else if (OperatingSystem.IsLinux()) {
            _systemCompat = new LinuxSystemCompat();
        }
        else
        {
            throw new InvalidOperationException("Unsupported platform");
        }
#else
        _systemCompat = Environment.Is64BitProcess ? new Windows64SystemCompat() : new Windows32SystemCompat();
#endif
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