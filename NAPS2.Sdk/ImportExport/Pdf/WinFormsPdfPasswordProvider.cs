using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.WinForms;

namespace NAPS2.ImportExport.Pdf
{
    public class WinFormsPdfPasswordProvider : IPdfPasswordProvider
    {
        private readonly IFormFactory formFactory;

        public WinFormsPdfPasswordProvider(IFormFactory formFactory)
        {
            this.formFactory = formFactory;
        }

        public bool ProvidePassword(string fileName, int attemptCount, out string password)
        {
            var passwordForm = formFactory.Create<FPdfPassword>();
            passwordForm.FileName = fileName;
            var dialogResult = passwordForm.ShowDialog();
            password = passwordForm.Password;
            return dialogResult == DialogResult.OK;
        }
    }
}