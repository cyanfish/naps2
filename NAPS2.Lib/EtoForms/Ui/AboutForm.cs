using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Remoting.Worker;
using NAPS2.Update;

namespace NAPS2.EtoForms.Ui;

public class AboutForm : EtoDialogBase
{
    private const string NAPS2_HOMEPAGE = "https://www.naps2.com";
    private const string ICONS_HOMEPAGE = "https://www.fatcow.com/free-icons";
    private const string DONATE_URL = "https://www.naps2.com/donate?src=about";

    private readonly Control _donateButton;
    private readonly UpdateChecker _updateChecker;
    private readonly CheckBox _enableDebugLogging = C.CheckBox(UiStrings.EnableDebugLogging);

    public AboutForm(Naps2Config config, IIconProvider iconProvider, UpdateChecker updateChecker)
        : base(config)
    {
        Title = UiStrings.AboutFormTitle;
        Icon = iconProvider.GetFormIcon("information_small");

        _donateButton = EtoPlatform.Current.AccessibleImageButton(
            Icons.btn_donate_LG.ToEtoImage(),
            UiStrings.Donate,
            () => ProcessHelper.OpenUrl(DONATE_URL));
        _enableDebugLogging.Checked = config.Get(c => c.EnableDebugLogging);
        _enableDebugLogging.CheckedChanged += (_, _) =>
        {
            config.User.Set(c => c.EnableDebugLogging, _enableDebugLogging.IsChecked());
        };

        _updateChecker = updateChecker;
    }

    protected override void BuildLayout()
    {
        FormStateController.Resizable = false;
        FormStateController.RestoreFormState = false;

        LayoutController.DefaultSpacing = 2;
        LayoutController.Content = L.Row(
            L.Column(new ImageView { Image = Icons.scanner_128.ToEtoImage() }).Padding(right: 4),
            L.Column(
                C.NoWrap(AssemblyHelper.Product),
                L.Row(
                    L.Column(
                        C.NoWrap(string.Format(MiscResources.Version, AssemblyHelper.Version)),
                        C.UrlLink(NAPS2_HOMEPAGE)
                    ),
                    Config.Get(c => c.HiddenButtons).HasFlag(ToolbarButtons.Donate)
                        ? C.None()
                        : L.Column(
                            C.Filler(),
                            _donateButton
                        ).Padding(left: 10)
                ),
                GetUpdateWidget(),
                C.TextSpace(),
                C.NoWrap(string.Format(UiStrings.CopyrightFormat, AssemblyHelper.COPYRIGHT_YEARS)),
                Config.AppLocked.Has(c => c.EnableDebugLogging)
                    ? C.None()
                    : new[] { C.Spacer(), _enableDebugLogging.Padding(left: 4) }.Expand(),
                C.TextSpace(),
                L.Row(
                    L.Column(
                        C.NoWrap(UiStrings.IconsFrom),
                        C.UrlLink(ICONS_HOMEPAGE)
                    ).Scale(),
                    L.Column(
                        C.Filler(),
                        C.DialogButton(this, UiStrings.OK, true, true)
                    ).Padding(left: 20)
                )
            )
        );
    }

    private LayoutElement GetUpdateWidget()
    {
#if MSI
        return C.None();
#else
#if NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindows()) return C.None();
#endif
        return new UpdateCheckWidget(_updateChecker, Config);
#endif
    }
}