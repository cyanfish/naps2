using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Targets;
using ILogger = NAPS2.Logging.ILogger;

namespace NAPS2;

public class NLogLogger : ILogger
{
    private readonly Logger _logger;

    public NLogLogger()
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
        _logger = LogManager.GetLogger("NAPS2");
    }

    public void Info(string message)
    {
        _logger.Info(message);
    }

    public void Error(string message)
    {
        _logger.Error(message);
    }

    public void ErrorException(string message, Exception exception)
    {
        _logger.Error(exception, message);
    }

    public void FatalException(string message, Exception exception)
    {
        _logger.Fatal(exception, message);
    }

    private class CustomExceptionLayoutRenderer : ExceptionLayoutRenderer
    {
        protected override void AppendToString(StringBuilder sb, Exception ex)
        {
            sb.AppendDemystified(ex);
        }
    }
}