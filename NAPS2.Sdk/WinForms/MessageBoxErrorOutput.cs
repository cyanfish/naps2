using System;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public class MessageBoxErrorOutput : ErrorOutput
    {
        private readonly DialogHelper _dialogHelper;

        public MessageBoxErrorOutput(DialogHelper dialogHelper)
        {
            _dialogHelper = dialogHelper;
        }

        public override void DisplayError(string errorMessage)
        {
            Invoker.Current.SafeInvoke(() => MessageBox.Show(errorMessage, MiscResources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error));
        }

        public override void DisplayError(string errorMessage, string details)
        {
            Invoker.Current.SafeInvoke(() => ShowErrorWithDetails(errorMessage, details));
        }

        public override void DisplayError(string errorMessage, Exception exception)
        {
            Invoker.Current.SafeInvoke(() => ShowErrorWithDetails(errorMessage, exception.ToString()));
        }
        private void ShowErrorWithDetails(string errorMessage, string details)
        {
            var form = new FError
            {
                ErrorMessage = errorMessage,
                Details = details
            };
            form.ShowDialog();
        }
    }
}