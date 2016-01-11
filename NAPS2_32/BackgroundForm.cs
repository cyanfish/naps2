using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NAPS2_32
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
