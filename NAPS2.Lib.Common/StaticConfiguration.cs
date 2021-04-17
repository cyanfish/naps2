using System;
using System.Diagnostics;
using NAPS2.Config;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Platform;
using Ninject;
using NLog;

namespace NAPS2
{
    public static class StaticConfiguration
    {
        public static void Initialize(IKernel kernel)
        {
            var config = kernel.Get<ScopedConfig>();

            Log.Logger = new NLogLogger();
            if (PlatformCompat.System.CanUseWin32)
            {
                Log.EventLogger = new WindowsEventLogger(config);
            }
#if DEBUG
            Debug.Listeners.Add(new NLogTraceListener());
#endif

            var customPath = config.Get(c => c.ComponentsPath);
            var basePath = string.IsNullOrWhiteSpace(customPath)
                ? Paths.Components
                : Environment.ExpandEnvironmentVariables(customPath);

            OcrEngineManager.Default = new OcrEngineManager(basePath);
        }
    }
}
