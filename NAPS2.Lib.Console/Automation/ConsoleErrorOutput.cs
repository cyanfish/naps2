using System;
using NAPS2.Util;

namespace NAPS2.Automation
{
    public class ConsoleErrorOutput : ErrorOutput
    {
        private readonly ConsoleOutput _output;

        public ConsoleErrorOutput(ConsoleOutput output)
        {
            _output = output;
        }

        public override void DisplayError(string errorMessage)
        {
            _output.Writer.WriteLine(errorMessage);
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
