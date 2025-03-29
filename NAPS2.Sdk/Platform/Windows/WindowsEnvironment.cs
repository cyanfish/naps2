using System.Text;

namespace NAPS2.Platform.Windows;

internal static class WindowsEnvironment
{
    private const long APPMODEL_ERROR_NO_PACKAGE = 15700L;

    public static bool IsRunningAsMsix
    {
        get
        {
#if NET6_0_OR_GREATER
            if (OperatingSystem.IsWindowsVersionAtLeast(10))
            {
                int length = 0;
                var sb = new StringBuilder(0);
                Win32.GetCurrentPackageFullName(ref length, sb);
                sb = new StringBuilder(length);
                int result = Win32.GetCurrentPackageFullName(ref length, sb);
                return result != APPMODEL_ERROR_NO_PACKAGE;
            }
#endif
            return false;
        }
    }
}