using NAPS2.Lang.Resources;
using NAPS2.Util;
using System;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class MessageBoxErrorOutput : IErrorOutput
    {
        private readonly DialogHelper dialogHelper;

        public MessageBoxErrorOutput(DialogHelper dialogHelper)
        {
            this.dialogHelper = dialogHelper;
        }

        public void DisplayError(string errorMessage)
        {
            MessageBox.Show(errorMessage, MiscResources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void DisplayError(string errorMessage, string details)
        {
            dialogHelper.ShowErrorWithDetails(errorMessage, details);
        }

        public void DisplayError(string errorMessage, Exception exception)
        {
            dialogHelper.ShowErrorWithDetails(errorMessage, exception.ToString());
        }
    }
}