﻿using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public class DonatePromptNotifyWidget : NotifyWidget
    {
        public DonatePromptNotifyWidget()
            : base(MiscResources.DonatePrompt, MiscResources.Donate, "https://www.naps2.com/donate", null)
        {
            HideTimer.Interval = 60 * 1000;
        }
    }
}