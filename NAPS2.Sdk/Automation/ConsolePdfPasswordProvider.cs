using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.ConsoleResources;
using NAPS2.Util;

namespace NAPS2.Automation
{
    public class ConsolePdfPasswordProvider : IPdfPasswordProvider
    {
        private readonly IErrorOutput errorOutput;

        public ConsolePdfPasswordProvider(IErrorOutput errorOutput)
        {
            this.errorOutput = errorOutput;
        }

        public bool ProvidePassword(string fileName, int attemptCount, out string password)
        {
            password = PasswordToProvide ?? "";
            if (attemptCount > 0)
            {
                errorOutput.DisplayError(PasswordToProvide == null
                    ? ConsoleResources.ImportErrorNoPassword : ConsoleResources.ImportErrorWrongPassword);
                return false;
            }
            return true;
        }

        public static string PasswordToProvide { get; set; }
    }
}