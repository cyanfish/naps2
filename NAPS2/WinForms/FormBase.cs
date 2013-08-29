using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ninject;

namespace NAPS2.WinForms
{
    public class FormBase : Form
    {
        public FormBase()
        {
            UpdateRTL();
        }

        public FormBase(IKernel kernel)
            : this()
        {
            Kernel = kernel;
            Load += OnLoad;
        }

        public IKernel Kernel { get; private set; }

        protected void UpdateRTL()
        {
            bool isRTL = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
            RightToLeft = isRTL ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = isRTL;
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {

        }
    }
}
