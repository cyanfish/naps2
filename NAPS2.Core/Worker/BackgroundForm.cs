using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.WinForms;

namespace NAPS2.Worker
{
    /// <summary>
    /// A basic implementation of an invisible form used in NAPS2.Worker.exe as a parent
    /// for any dialogs that may need to be displayed.
    /// </summary>
    public class BackgroundForm : FormBase
    {
        public BackgroundForm()
        {
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            WindowState = FormWindowState.Minimized;
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!IsHandleCreated)
            {
                CreateHandle();
                value = false;
            }
            base.SetVisibleCore(value);
        }
    }
}
