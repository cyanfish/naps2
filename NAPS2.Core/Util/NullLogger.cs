﻿using System;

namespace NAPS2.Util
{
    public class NullLogger : ILogger
    {
        public void Error(string message)
        {
        }

        public void ErrorException(string message, Exception exception)
        {
        }

        public void FatalException(string message, Exception exception)
        {
        }
    }
}