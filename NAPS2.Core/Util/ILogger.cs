using System;

namespace NAPS2.Util
{
    public interface ILogger
    {
        void Error(string message);

        void ErrorException(string message, Exception exception);

        void FatalException(string message, Exception exception);
    }
}