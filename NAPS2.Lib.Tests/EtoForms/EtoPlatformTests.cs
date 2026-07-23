using Eto.Forms;
using NAPS2.EtoForms;
using Xunit;

namespace NAPS2.Lib.Tests.EtoForms;

public class EtoPlatformTests
{
    [Fact]
    public void PlatformShortcutTakesPrecedence()
    {
        var appShortcutCalled = false;

        var handled = EtoPlatform.RouteKeyDown(
            Keys.Application | Keys.Q,
            () => true,
            _ =>
            {
                appShortcutCalled = true;
                return true;
            });

        Assert.True(handled);
        Assert.False(appShortcutCalled);
    }

    [Fact]
    public void AppShortcutHandlesNonPlatformShortcut()
    {
        var handled = EtoPlatform.RouteKeyDown(
            Keys.F2,
            () => false,
            keys => keys == Keys.F2);

        Assert.True(handled);
    }

    [Fact]
    public void UnmatchedShortcutRemainsUnhandled()
    {
        var handled = EtoPlatform.RouteKeyDown(
            Keys.F2,
            () => false,
            _ => false);

        Assert.False(handled);
    }
}
