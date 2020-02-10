using System;
using NAPS2.Util;

namespace NAPS2.Automation
{
    public class ConsoleErrorOutput : ErrorOutput
    {
        private readonly ConsoleOutput output;

        public ConsoleErrorOutput(ConsoleOutput output)
        {
            this.output = output;
        }

        public override void DisplayError(string errorMessage)
        {
            output.Writer.WriteLine(errorMessage);
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
