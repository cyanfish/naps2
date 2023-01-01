namespace NAPS2.Platform;

public class PlatformCompat
{
    private static ISystemCompat _systemCompat;

    static PlatformCompat()
    {
        // TODO: This might be wrong for netstandard
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            _systemCompat = GetWindowsSystemCompat();
        }
        else if (OperatingSystem.IsMacOS())
        {
            _systemCompat = new MacSystemCompat();
        }
        else if (OperatingSystem.IsLinux())
        {
            _systemCompat = new LinuxSystemCompat();
        }
        else
        {
            throw new InvalidOperationException("Unsupported platform");
        }
#else
        _systemCompat = GetWindowsSystemCompat();
#endif
    }

    private static ISystemCompat GetWindowsSystemCompat() =>
        Environment.Is64BitProcess
            ? new Windows64SystemCompat()
            : new Windows32SystemCompat();

    public static ISystemCompat System
    {
        get => _systemCompat;
        set => _systemCompat = value ?? throw new ArgumentNullException(nameof(value));
    }
}