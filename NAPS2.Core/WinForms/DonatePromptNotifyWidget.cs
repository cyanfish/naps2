using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class DonatePromptNotifyWidget : NotifyWidget
    {
        public DonatePromptNotifyWidget()
            : base(MiscResources.DonatePrompt, MiscResources.Donate, "https://www.naps2.com/donate", null)
        {
            hideTimer.Interval = 60 * 1000;
        }

        public override NotifyWidgetBase Clone() => new DonatePromptNotifyWidget();
    }
}
