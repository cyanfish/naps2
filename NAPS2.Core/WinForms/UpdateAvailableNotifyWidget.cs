using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Update;

namespace NAPS2.WinForms
{
    public class UpdateAvailableNotifyWidget : NotifyWidget
    {
        private readonly UpdateChecker updateChecker;
        private readonly UpdateInfo update;

        public UpdateAvailableNotifyWidget(UpdateChecker updateChecker, UpdateInfo update)
            : base(MiscResources.UpdateAvailable, string.Format(MiscResources.Install, update.Name), null, null)
        {
            this.updateChecker = updateChecker;
            this.update = update;

            hideTimer.Interval = 60 * 1000;
        }

        public override NotifyWidgetBase Clone() => new UpdateAvailableNotifyWidget(updateChecker, update);

        protected override void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            updateChecker.StartUpdate(update);
            DoHideNotify();
        }
    }
}
