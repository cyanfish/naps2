﻿using NAPS2.Util;
using System;

namespace NAPS2.Automation
{
    public class ConsoleErrorOutput : IErrorOutput
    {
        public void DisplayError(string errorMessage)
        {
            Console.WriteLine(errorMessage);
        }

        public void DisplayError(string errorMessage, string details)
        {
            DisplayError(errorMessage);
        }

        public void DisplayError(string errorMessage, Exception exception)
        {
            DisplayError(errorMessage);
        }
    }
}