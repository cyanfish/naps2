using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.WinForms;
using WIA;

namespace NAPS2.Scan.Wia
{
    public class WinFormsWiaTransfer : IWiaTransfer
    {
        private readonly IFormFactory formFactory;

        public WinFormsWiaTransfer(IFormFactory formFactory)
        {
            this.formFactory = formFactory;
        }

        public ImageFile Transfer(int pageNumber, Item item, string format)
        {
            if (pageNumber == 1)
            {
                // The only downside of the common dialog is that it steals focus.
                // If this is the first page, then the user has just pressed the scan button, so that's not
                // an issue and we can use it and get the benefits of progress display and immediate cancellation.
                return (ImageFile)new CommonDialogClass().ShowTransfer(item, format, false);
            }
            // For subsequent pages, we don't want to take focus in case the user has switched applications,
            // so we use the custom form.
            var form = formFactory.Create<FScanProgress>();
            form.PageNumber = pageNumber;
            form.Item = item;
            form.Format = format;
            form.ShowDialog();
            if (form.DialogResult == DialogResult.Cancel)
            {
                return null;
            }
            return form.ImageFile;
        }
    }
}