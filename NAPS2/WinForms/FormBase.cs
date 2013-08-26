using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class FormBase : Form
    {
        public FormBase()
        {
            UpdateRTL();
        }

        protected void UpdateRTL()
        {
            bool isRTL = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
            RightToLeft = isRTL ? RightToLeft.Yes : RightToLeft.No;
        }
    }
}
