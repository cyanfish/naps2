using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
