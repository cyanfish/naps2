#if WINDOWS
using NAPS2.Images.Gdi;
using NAPS2.Platform.Windows;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Lib.Tests.Platform;

[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
public class WindowsIconsTests : ContextualTests
{
    private static readonly string NotepadPath = Path.Combine(Environment.SystemDirectory, "notepad.exe");

    [PlatformFact(include: PlatformFlags.Windows)]
    public void ExtractAssociatedIcon()
    {
        using var icon = WindowsIcons.ExtractAssociatedIcon(ImageContext, NotepadPath);

        Assert.NotNull(icon);
        Assert.Equal(ImagePixelFormat.ARGB32, icon.PixelFormat);
        // Compare with System.Drawing's implementation
        using var expected = new GdiImage(System.Drawing.Icon.ExtractAssociatedIcon(NotepadPath)!.ToBitmap());
        ImageAsserts.Similar(expected, icon, ignoreResolution: true);
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public void ExtractIcon()
    {
        using var icon = WindowsIcons.ExtractIcon(ImageContext, NotepadPath, 0);

        Assert.NotNull(icon);
        Assert.Equal(ImagePixelFormat.ARGB32, icon.PixelFormat);
        using var expected = new GdiImage(System.Drawing.Icon.ExtractIcon(NotepadPath, 0)!.ToBitmap());
        ImageAsserts.Similar(expected, icon, ignoreResolution: true);
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public void ExtractIconWithInvalidIndex()
    {
        Assert.Null(WindowsIcons.ExtractIcon(ImageContext, NotepadPath, 10000));
    }

    [PlatformFact(include: PlatformFlags.Windows)]
    public void ExtractIconFromMissingFile()
    {
        Assert.Null(WindowsIcons.ExtractIcon(ImageContext, P("missing.exe"), 0));
    }
}
#endif
