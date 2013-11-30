using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NAPS2
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
            logger.ErrorException(message, exception);
        }
    }
}
