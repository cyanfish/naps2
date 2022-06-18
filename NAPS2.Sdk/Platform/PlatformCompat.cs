namespace NAPS2.Platform;

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
            _systemCompat = Environment.Is64BitProcess ? new Windows64SystemCompat() : new Windows32SystemCompat();
        }
        else if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            _systemCompat = new LinuxSystemCompat();
        }
        else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
        {
            _systemCompat = new MacSystemCompat();
        }
        else
        {
            throw new InvalidOperationException("Unsupported platform");
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