using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
{
    public class Log
    {
        private static ILogger _logger = new NullLogger();

        public static ILogger Logger
        {
            get => _logger;
            set => _logger = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static void Error(string message, params object[] args)
        {
            _logger.Error(string.Format(message, args));
        }

        public static void ErrorException(string message, Exception exception)
        {
            _logger.ErrorException(message, exception);
        }

        public static void FatalException(string message, Exception exception)
        {
            _logger.FatalException(message, exception);
        }
    }
}
