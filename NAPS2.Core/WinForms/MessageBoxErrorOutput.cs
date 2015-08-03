using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public class MessageBoxErrorOutput : IErrorOutput
    {
        public void DisplayError(string errorMessage)
        {
            MessageBox.Show(errorMessage, MiscResources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}