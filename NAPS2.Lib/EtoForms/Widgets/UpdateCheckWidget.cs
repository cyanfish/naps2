using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Update;

namespace NAPS2.EtoForms.Widgets;

public class UpdateCheckWidget
{
    private readonly UpdateChecker _updateChecker;
    private readonly Naps2Config _config;
    private readonly CheckBox _checkForUpdates;
    private readonly Panel _updatePanel;
    private bool _hasCheckedForUpdates;
    private UpdateInfo? _update;

    public UpdateCheckWidget(UpdateChecker updateChecker, Naps2Config config)
    {
        _updateChecker = updateChecker;
        _config = config;
        _checkForUpdates = new CheckBox { Text = UiStrings.CheckForUpdates };
        _checkForUpdates.Checked = config.Get(c => c.CheckForUpdates);
        _checkForUpdates.CheckedChanged += CheckForUpdatesChanged;
        _updatePanel = new Panel();
        UpdateControls();
        DoUpdateCheck();
    }

    public LayoutColumn AsControl()
    {
        return L.Column(
            C.TextSpace(),
            _checkForUpdates.Padding(left: 4),
            _updatePanel
        );
    }

    public static implicit operator LayoutElement(UpdateCheckWidget control)
    {
        return control.AsControl();
    }

    private void DoUpdateCheck()
    {
        if (_checkForUpdates.IsChecked())
        {
            _updateChecker.CheckForUpdates().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Log.ErrorException("Error checking for updates", task.Exception!);
                }
                else
                {
                    var transact = _config.User.BeginTransaction();
                    transact.Set(c => c.HasCheckedForUpdates, true);
                    transact.Set(c => c.LastUpdateCheckDate, DateTime.Now);
                    transact.Commit();
                }
                _update = task.Result;
                _hasCheckedForUpdates = true;
                Invoker.Current.Invoke(UpdateControls);
            });
        }
    }

    private void UpdateControls()
    {
        _updatePanel.Content = GetUpdatePanelContent();
    }

    private Control GetUpdatePanelContent()
    {
        if (!_checkForUpdates.IsChecked())
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

    private void CheckForUpdatesChanged(object? sender, EventArgs e)
    {
        _config.User.Set(c => c.CheckForUpdates, _checkForUpdates.IsChecked());
        UpdateControls();
        DoUpdateCheck();
    }
}