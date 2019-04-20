using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Automation
{
    public class ConsoleErrorOutput : ErrorOutput
    {
        public override void DisplayError(string errorMessage)
        {
            Console.WriteLine(errorMessage);
        }

        public override void DisplayError(string errorMessage, string details)
        {
            DisplayError(errorMessage);
        }

        public override void DisplayError(string errorMessage, Exception exception)
        {
            DisplayError(errorMessage);
        }
    }
}
