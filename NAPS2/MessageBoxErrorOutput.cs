using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;

namespace NAPS2
{
    public class MessageBoxErrorOutput : IErrorOutput
    {
        public void DisplayError(string errorMessage)
        {
            MessageBox.Show(errorMessage, MiscResources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}