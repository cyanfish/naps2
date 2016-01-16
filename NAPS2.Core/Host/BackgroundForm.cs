using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Host
{
    public class BackgroundForm : Form
    {
        public BackgroundForm()
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
        }
    }
}
