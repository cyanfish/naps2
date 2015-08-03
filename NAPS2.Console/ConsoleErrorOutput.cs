using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Console
{
    public class ConsoleErrorOutput : IErrorOutput
    {
        public void DisplayError(string errorMessage)
        {
            System.Console.WriteLine(errorMessage);
        }
    }
}
