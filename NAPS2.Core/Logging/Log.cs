using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Logging
{
    /// <summary>
    /// Logging functionality.
    /// </summary>
    public static class Log
    {
        private static ILogger _logger = new NullLogger();
        private static IEventLogger _eventLogger = new NullEventLogger();

        public static ILogger Logger
        {
            get => _logger;
            set => _logger = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static IEventLogger EventLogger
        {
            get => _eventLogger;
            set => _eventLogger = value ?? throw new ArgumentNullException(nameof(value));
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

        public static void Event(EventType eventType, Event evt)
        {
            _eventLogger.LogEvent(eventType, evt);
        }
    }
}
