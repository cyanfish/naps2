using System.Windows.Forms;

namespace NAPS2.Host
{
    public class BackgroundForm : Form
    {
        public BackgroundForm()
        {
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            WindowState = FormWindowState.Minimized;
        }
    }
}