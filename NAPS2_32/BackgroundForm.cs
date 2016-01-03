using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NAPS2_32
{
    public class BackgroundForm : Form
    {
        private readonly string pipeName;

        public BackgroundForm(string pipeName)
        {
            this.pipeName = pipeName;
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;

            Load += BackgroundForm_Load;
        }

        void BackgroundForm_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Listening at " + pipeName);
            Close();
        }
    }
}
