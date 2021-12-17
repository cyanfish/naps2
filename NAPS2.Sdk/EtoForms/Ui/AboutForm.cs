using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Eto.Forms;
using Eto.WinForms;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Threading;
using NAPS2.Update;

namespace NAPS2.EtoForms.Ui;

public class AboutForm : EtoDialogBase
{
    private const string NAPS2_HOMEPAGE = "https://www.naps2.com";  
    private const string ICONS_HOMEPAGE = "https://www.fatcow.com/free-icons";
    private const string DONATE_URL = "https://www.naps2.com/donate";

    private readonly UpdateChecker _updateChecker;
        
    private readonly Control _donateButton;
    private readonly CheckBox _checkForUpdates;
    private readonly Panel _updatePanel;
        
    private bool _hasCheckedForUpdates;
    private UpdateInfo? _update;

    public AboutForm(ScopedConfig config, UpdateChecker updateChecker)
        : base(config)
    {
        _updateChecker = updateChecker;
            
        Title = UiStrings.AboutFormTitle;
        Icon = Icons.information_small.ToEtoIcon();
        Resizable = false;
        FormStateController.RestoreFormState = false;
            
        _donateButton = C.AccessibleImageButton(
            Icons.btn_donate_LG.ToEto(),
            UiStrings.Donate,
            () => Process.Start(DONATE_URL));
        _checkForUpdates = new CheckBox { Text = UiStrings.CheckForUpdates };
        _checkForUpdates.CheckedChanged += CheckForUpdatesChanged;
        _updatePanel = new Panel();
            
        BuildLayout();
        UpdateControls();
    }

    private void BuildLayout()
    {
        Content = L.Root(
            L.Row(
                L.Column(new ImageView { Image = Icons.scanner_large.ToEto() }).Padding(right: 4),
                L.Column(
                    C.NoWrap(AssemblyProduct),
                    L.Row(
                        L.Column(
                            C.NoWrap(string.Format(MiscResources.Version, AssemblyVersion)),
                            C.Link(NAPS2_HOMEPAGE)
                        ),
                        L.Column(
                            C.ZeroSpace().YScale(),
                            _donateButton
                        ).Padding(left: 10)
                    ),
                    C.TextSpace(),
                    _checkForUpdates.AutoSize().Padding(left: 4),
                    _updatePanel,
                    C.TextSpace(),
                    C.NoWrap(UiStrings.Copyright),
                    C.TextSpace(),
                    L.Row(
                        L.Column(
                            C.NoWrap(UiStrings.IconsFrom),
                            C.Link(ICONS_HOMEPAGE)
                        ).XScale(),
                        L.Column(
                            C.ZeroSpace().YScale(),
                            C.Button(UiStrings.OK, Close)
                        ).Padding(left: 20)
                    ),
                    C.ZeroSpace()
                )
            )
        ).DefaultSpacing(2);
    }
        
    private void DoUpdateCheck()
    {
        if (_checkForUpdates.Checked == true)
        {
            _updateChecker.CheckForUpdates().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Log.ErrorException("Error checking for updates", task.Exception);
                }
                else
                {
                    Config.User.SetAll(new CommonConfig
                    {
                        HasCheckedForUpdates = true,
                        LastUpdateCheckDate = DateTime.Now
                    });
                }
                _update = task.Result;
                _hasCheckedForUpdates = true;
                Invoker.Current.SafeInvoke(UpdateControls);
            });
        }
    }

    private void UpdateControls()
    {
        _updatePanel.Content = GetUpdatePanelContent();
    }

    private Control GetUpdatePanelContent()
    {
        if (_checkForUpdates.Checked != true)
        {
            return C.NoWrap(MiscResources.UpdateCheckDisabled);
        }
        if (!_hasCheckedForUpdates)
        {
            return C.NoWrap(MiscResources.CheckingForUpdates);
        }
        if (_update == null)
        {
            return C.NoWrap(MiscResources.NoUpdates);
        }
        return C.Link(string.Format(MiscResources.Install, _update.Name),
            InstallLinkClicked);
    }

    private void InstallLinkClicked()
    {
        if (_update != null)
        {
            _updateChecker.StartUpdate(_update);
        }
    }

    private void CheckForUpdatesChanged(object sender, EventArgs e)
    {
        Config.User.Set(c => c.CheckForUpdates = _checkForUpdates.Checked);
        UpdateControls();
        DoUpdateCheck();
    }

    // TODO: Move to a utility class
    #region Assembly Attribute Accessors

    private static string GetAssemblyAttributeValue<T>(Func<T, string> selector)
    {
        object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(T), false);
        if (attributes.Length == 0)
        {
            return "";
        }
        return selector((T)attributes[0]);
    }

    public string AssemblyTitle
    {
        get
        {
            string title = GetAssemblyAttributeValue<AssemblyTitleAttribute>(x => x.Title);
            if (string.IsNullOrEmpty(title))
            {
                title = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
            }
            return title;
        }
    }

    public string AssemblyVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();

    public string AssemblyDescription => GetAssemblyAttributeValue<AssemblyDescriptionAttribute>(x => x.Description);

    public string AssemblyProduct => GetAssemblyAttributeValue<AssemblyProductAttribute>(x => x.Product);

    public string AssemblyCopyright => GetAssemblyAttributeValue<AssemblyCopyrightAttribute>(x => x.Copyright);

    public string AssemblyCompany => GetAssemblyAttributeValue<AssemblyCompanyAttribute>(x => x.Company);

    #endregion
}