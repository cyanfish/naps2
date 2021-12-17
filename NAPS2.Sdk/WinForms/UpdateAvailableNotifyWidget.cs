using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Update;

namespace NAPS2.WinForms;

public class UpdateAvailableNotifyWidget : NotifyWidget
{
    private readonly UpdateChecker _updateChecker;
    private readonly UpdateInfo _update;

    public UpdateAvailableNotifyWidget(UpdateChecker updateChecker, UpdateInfo update)
        : base(MiscResources.UpdateAvailable, string.Format(MiscResources.Install, update.Name), null, null)
    {
        _updateChecker = updateChecker;
        _update = update;

        hideTimer.Interval = 60 * 1000;
    }

    public override NotifyWidgetBase Clone() => new UpdateAvailableNotifyWidget(_updateChecker, _update);

    protected override void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        _updateChecker.StartUpdate(_update);
        DoHideNotify();
    }
}