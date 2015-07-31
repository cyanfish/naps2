using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Pdf
{
    public class ConsolePdfPasswordProvider : IPdfPasswordProvider
    {
        public bool ProvidePassword(string fileName, out string password)
        {
            password = PasswordToProvide ?? "";
            return true;
        }

        public static string PasswordToProvide { get; set; }
    }
}