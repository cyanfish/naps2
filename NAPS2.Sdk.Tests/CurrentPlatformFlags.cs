using System.Runtime.InteropServices;

namespace NAPS2.Sdk.Tests;

public static class CurrentPlatformFlags
{
    public static PlatformFlags Get()
    {
        var p = PlatformFlags.None;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            p |= PlatformFlags.Windows;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            p |= PlatformFlags.Mac;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            p |= PlatformFlags.Linux;
        }
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            p |= PlatformFlags.X64;
        }
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            p |= PlatformFlags.Arm64;
        }
        if (TestImageContextFactory.Get().ImageType.Name == "ImageSharpImage")
        {
            p |= PlatformFlags.ImageSharpImage;
        }
        if (TestImageContextFactory.Get().ImageType.Name == "WpfImage")
        {
            p |= PlatformFlags.WpfImage;
        }
        if (TestImageContextFactory.Get().ImageType.Name == "GdiImage")
        {
            p |= PlatformFlags.GdiImage;
        }
        if (TestImageContextFactory.Get().ImageType.Name == "MacImage")
        {
            p |= PlatformFlags.MacImage;
        }
        if (TestImageContextFactory.Get().ImageType.Name == "GtkImage")
        {
            p |= PlatformFlags.GtkImage;
        }
        return p;
    }

    public static bool Has(PlatformFlags match)
    {
        var flags = Get();
        return (match & flags) == match;
    }

    public static bool HasAny(PlatformFlags match)
    {
        var flags = Get();
        return (match & flags) != 0;
    }
}