using System.IO;
using Ninject.Activation;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NAPS2
{
    public static class LoggerFactory
    {
        public static Logger GetLogger(IContext arg)
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
            var rule = new LoggingRule("*", LogLevel.Trace, target);
            config.LoggingRules.Add(rule);
            LogManager.Configuration = config;
            return LogManager.GetLogger("NAPS2");
        }
    }
}