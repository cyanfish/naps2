using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using ILogger = NAPS2.Logging.ILogger;

namespace NAPS2.DI
{
    public class NLogLogger : ILogger
    {
        private readonly Logger logger;

        public NLogLogger()
        {
            var config = new LoggingConfiguration();
            var target = new FileTarget
            {
                FileName = Path.Combine(Paths.AppData, "errorlog.txt"),
                Layout = "${longdate} ${message} ${exception:format=tostring}",
                ArchiveAboveSize = 100000,
                MaxArchiveFiles = 5
            };
            config.AddTarget("errorlogfile", target);
            var rule = new LoggingRule("*", LogLevel.Debug, target);
            config.LoggingRules.Add(rule);
            LogManager.Configuration = config;
            logger = LogManager.GetLogger("NAPS2");
        }

        public void Error(string message)
        {
            logger.Error(message);
        }

        public void ErrorException(string message, Exception exception)
        {
            logger.Error(exception, message);
        }

        public void FatalException(string message, Exception exception)
        {
            logger.Fatal(exception, message);
        }
    }
}
