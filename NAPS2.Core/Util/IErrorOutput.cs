﻿using System;

namespace NAPS2.Util
{
    public interface IErrorOutput
    {
        void DisplayError(string errorMessage);

        void DisplayError(string errorMessage, string details);

        void DisplayError(string errorMessage, Exception exception);
    }
}