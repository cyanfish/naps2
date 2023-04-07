using System.Text;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.LayoutRenderers;
using NLog.Targets;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NAPS2;

public static class NLogConfig
{
    public static ILogger CreateLogger()
    {
        LayoutRenderer.Register<CustomExceptionLayoutRenderer>("exception");
        var config = new LoggingConfiguration();
        var target = new FileTarget
        {
            FileName = Path.Combine(Paths.AppData, "errorlog.txt"),
            Layout = "${longdate} ${processid} ${message} ${exception:format=tostring}",
            ArchiveAboveSize = 100000,
            MaxArchiveFiles = 5
        };
        config.AddTarget("errorlogfile", target);
        var rule = new LoggingRule("*", LogLevel.Debug, target);
        config.LoggingRules.Add(rule);
        LogManager.Configuration = config;
        return new NLogLoggerFactory().CreateLogger("NAPS2");
    }

    private class CustomExceptionLayoutRenderer : ExceptionLayoutRenderer
    {
        protected override void AppendToString(StringBuilder sb, Exception ex)
        {
            // Note we don't want to use the AppendDemystified() helper
            // https://github.com/benaadams/Ben.Demystifier/issues/85
            sb.Append(ex.Demystify());
        }
    }
}