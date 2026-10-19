using System.Threading;
using System.Windows.Threading;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Widgets;
using NAPS2.EtoForms.WinForms;
using SW = System.Windows;

namespace NAPS2.EtoForms.Wpf;

public class WpfEtoPlatform : EtoPlatform
{
    public override bool IsWpf => true;

    public override IIconProvider IconProvider { get; } = new DefaultIconProvider();

    public override IDarkModeProvider DarkModeProvider { get; } = new WindowsDarkModeProvider();

    public override Application CreateApplicationCore()
    {
        // Eto.Wpf reuses an existing System.Windows.Application if there is one, so we create it first to configure
        // the Fluent theme before any windows are created.
        _ = new SW.Application { ShutdownMode = SW.ShutdownMode.OnExplicitShutdown };
        var application = new Application(Eto.Platforms.Wpf);
        SynchronizationContext.SetSynchronizationContext(
            new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
        SetSystemTheme();
        return application;
    }

    public override void SetSystemTheme()
    {
        if (SW.Application.Current is { } app)
        {
            app.ThemeMode = ColorScheme.DarkMode ? SW.ThemeMode.Dark : SW.ThemeMode.Light;
        }
    }

    public override IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        throw new NotImplementedException();

    public override ISettingsNavigator CreateSettingsNavigator() => throw new NotImplementedException();

    public override void ConfigureImageButton(Button button, ButtonFlags flags) =>
        throw new NotImplementedException();

    public override Bitmap ToBitmap(IMemoryImage image) => throw new NotImplementedException();

    public override IMemoryImage FromBitmap(Bitmap bitmap) => throw new NotImplementedException();

    public override IMemoryImage DrawHourglass(IMemoryImage thumb) => throw new NotImplementedException();

    public override void SetFrame(Control container, Control control, Point location, Size size, bool inOverlay) =>
        throw new NotImplementedException();

    public override Control CreateContainer() => throw new NotImplementedException();

    public override void AddToContainer(Control container, Control control, bool inOverlay) =>
        throw new NotImplementedException();

    public override void RemoveFromContainer(Control container, Control control) =>
        throw new NotImplementedException();

    public override Control CreateScrollContainer() => throw new NotImplementedException();

    public override Control GetScrollContentContainer(Control scrollContainer) =>
        throw new NotImplementedException();

    public override void SetScrollContentSize(Control scrollContainer, Size size) =>
        throw new NotImplementedException();

    public override int GetVerticalScrollBarWidth(Control scrollContainer) => throw new NotImplementedException();

    public override void ConfigureDonateButton(Button button) => throw new NotImplementedException();
}
